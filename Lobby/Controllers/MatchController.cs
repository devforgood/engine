using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lobby.Filter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;

namespace Lobby.Controllers
{
    [SerializeActionFilter]
    public class MatchController : Controller
    {
        private static TimeSpan user_state_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan match_user_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan match_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan startplay_polling_period = new TimeSpan(0, 0, 5);


        private static int MAX_START_PLAYER_COUNT = 4;

        public IActionResult Index()
        {
            var server_info_list = new List<ServerCommon.ServerInfo>();


            var db = Cache.Instance.GetDatabase();
            var entry = db.HashGetAll(ServerCommon.ServerInfoRedisKey.server_info);
            for (int i = 0; i < entry.Length; ++i)
            {
                server_info_list.Add(JsonConvert.DeserializeObject<ServerCommon.ServerInfo>(entry[i].Value));
            }

            var svr = Cache.Instance.GetServer();
            foreach (var key in svr.Keys(pattern: "battle:*"))
            {
                var user_count = db.StringGet(key);
            }


            return new JsonResult(server_info_list);
            //return new EmptyResult();
        }

        public bool GetAvailableServer(out string server_addr, out byte world_id, out string channel_key)
        {
            var db = Cache.Instance.GetDatabase();
            var entry = db.HashGetAll("channel_info");
            for (int i = 0; i < entry.Length; ++i)
            {
                var ch = JsonConvert.DeserializeObject<ServerCommon.Channel>(entry[i].Value);

                var channel_state = db.StringGet(ch.channel_id);
                if (channel_state.HasValue == true && ((ServerCommon.ChannelState)((int)channel_state)) == ServerCommon.ChannelState.CHL_READY )
                {
                    Log.Information(string.Format("GetAvailableServer {0}, {1}, {2}", ch.channel_id, channel_state, entry[i].Name));

                    server_addr = ch.server_addr;
                    world_id = ch.world_id;
                    channel_key = entry[i].Name;
                    return true;
                }
            }

            server_addr = "";
            world_id = 0;
            channel_key = "";

            return false;
        }

        public void RemoveMatchUser(long user_no)
        {
            var db = Cache.Instance.GetDatabase();
            db.SortedSetRemove("waiting_list",  user_no);
            // match_user 는 삭제 하지 않는다. 만약 삭제하게되면 
            // waiting_list를 얻은 상태에서 match_user를 선점하게되어 이미 게임 시작 중인 유저가 다시 매칭될수 있다.
            
        }

        ServerCommon.Channel GetChannel(long match_id)
        {
            var db = Cache.Instance.GetDatabase();
            var match_value = db.StringGet(string.Format("match:{0}", match_id));
            if(match_value.HasValue)
            {
                var channel_value = db.HashGet("channel_info", match_value);
                if (channel_value.HasValue)
                {
                    return JsonConvert.DeserializeObject<ServerCommon.Channel>(channel_value);
                }
                else
                {
                    Log.Warning("cannot find channel " + match_value);
                }
            }
            else
            {
                Log.Warning("cannot find match " + match_id);
            }
            return null;
        }



        public IActionResult StartPlay()
        {
            var session = this.GetSession();
            if (session == null)
            {
                Log.Warning("lost session");
                return null;
            }

            var response = new core.StartPlay();

            var db = Cache.Instance.GetDatabase();

            // 다른 유저가 매칭 시도를 했을 경우 
            var value = db.StringGet(string.Format("match_user:{0}", session.user_no));
            if (value.HasValue)
            {
                // 매칭 시도가 성공했을 경우
                var ch = GetChannel((long)value);
                if (ch != null)
                {
                    RemoveMatchUser(session.user_no);

                    response.is_start = true;
                    response.battle_server_addr = ch.server_addr;
                    response.wait_time_sec = 0;
                    response.world_id = ch.world_id;

                    Log.Information(string.Format("StartPlay {0}", session.user_no));
                    return new JsonResult(response);
                }
                else
                {
                    //db.KeyDelete(string.Format("match_user:{0}", session.user_no));

                    response.is_start = false;
                    response.battle_server_addr = "";
                    response.wait_time_sec = 1;
                    return new JsonResult(response);
                }
            }

            long match_id = 0;
            var player_list = new List<long>();
            var waiting_list = db.SortedSetRangeByScore("waiting_list");
            if (waiting_list.Length > MAX_START_PLAYER_COUNT - 1)
            {
                match_id = db.StringIncrement("match_instance_id");
                for (int i = 0; i < waiting_list.Length; ++i)
                {
                    long user_no = (long)waiting_list[i];
                    // 자신은 스킵
                    if (user_no == session.user_no)
                        continue;

                    if (db.StringGet(string.Format("waiting:{0}", user_no)).HasValue)
                    {
                        // 매칭에 필요한 유저를 선점한다
                        if (db.StringSet(string.Format("match_user:{0}", user_no), match_id, match_user_expire, When.NotExists) == true)
                        {
                            player_list.Add(user_no);
                            Log.Information(string.Format("Candidate User {0}", user_no));

                            if (player_list.Count == MAX_START_PLAYER_COUNT - 1)
                                break;
                        }
                    }
                    else
                    {
                        // 유효하지 않는 유저는 대기자 목록에서 삭제한다.
                        RemoveMatchUser(user_no);
                    }
                }
            }

            if (player_list.Count == MAX_START_PLAYER_COUNT - 1)
            {
                // 매칭에 필요한 인원을 모두 찾았을때
                // 전투 가능한 서버를 찾아 세팅
                string server_addr;
                byte worldId;
                string channel_key;
                if (GetAvailableServer(out server_addr, out worldId, out channel_key))
                {
                    db.StringSet(string.Format("match:{0}", match_id), channel_key, match_expire);

                    RemoveMatchUser(session.user_no);

                    response.is_start = true;
                    response.battle_server_addr = server_addr;
                    response.wait_time_sec = 0;
                    response.world_id = worldId;

                    Log.Information(string.Format("StartPlay {0}", session.user_no));
                    return new JsonResult(response);
                }
                else
                {
                    // 전투 가능한 서버가 없다
                }
            }


            // 매칭 시도를 실패하면 선점한 유저를 삭제
            db.KeyDelete(player_list.Select(key => (RedisKey)string.Format("match_user:{0}", key)).ToArray());


            // 대기자로 등록
            db.SortedSetAdd("waiting_list", session.user_no, session.rating);
            db.StringSet(string.Format("waiting:{0}", session.user_no), 0, startplay_polling_period);

            response.is_start = false;
            response.battle_server_addr = "";
            response.wait_time_sec = (int)startplay_polling_period.TotalSeconds;
            return new JsonResult(response);
        }


    }
}