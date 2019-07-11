using GameService;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lobby
{
    public class LobbyService : GameService.Lobby.LobbyBase
    {
        private static TimeSpan user_state_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan match_user_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan match_expire = new TimeSpan(0, 5, 0);
        private static TimeSpan startplay_polling_period = new TimeSpan(0, 0, 1);
        private static TimeSpan channel_reserve_expire = new TimeSpan(0, 1, 0);

        private static int MAX_START_PLAYER_COUNT = 4;

        JsonFormatter json_formatter = new JsonFormatter(new JsonFormatter.Settings(true));

        public LobbyService()
        {

 
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.Run( () =>
            {
                string msg = "empty";
                return Task.FromResult(new HelloReply { Message = msg });

            });

            //return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }

        public override async Task Login(LoginRequest request, IServerStreamWriter<LoginReply> responseStream, ServerCallContext context)
        {
            var session = await Session.CreateSession(context.Peer);

            await responseStream.WriteAsync(new LoginReply()
            {
                SessionId = session.session_id
            });
        }



        public async Task<(bool, string, byte, string)> GetAvailableServer()
        {
            var db = Cache.Instance.GetDatabase();
            var entry = await db.HashGetAllAsync("channel_info");
            for (int i = 0; i < entry.Length; ++i)
            {
                var ch = JsonConvert.DeserializeObject<ServerCommon.Channel>(entry[i].Value);

                var channel_state = await db.StringGetAsync(ch.channel_id);
                if (channel_state.HasValue == true && ((ServerCommon.ChannelState)((int)channel_state)) == ServerCommon.ChannelState.CHL_READY)
                {
                    // 해당 체널에 예약이 성공했을 경우만 처리
                    if (await db.StringSetAsync(string.Format("{0}:reserve", ch.channel_id), 0, channel_reserve_expire, When.NotExists) == true)
                    {
                        Log.Information(string.Format("GetAvailableServer {0}, {1}, {2}", ch.channel_id, channel_state, entry[i].Name));
                        return (true, ch.server_addr, ch.world_id, entry[i].Name);
                    }
                }
            }
            return (false, "", 0, "") ;
        }

        public async Task RemoveMatchUser(long user_no)
        {
            var db = Cache.Instance.GetDatabase();
            await db.SortedSetRemoveAsync("waiting_list", user_no);
            // match_user 는 삭제 하지 않는다. 만약 삭제하게되면 
            // waiting_list를 얻은 상태에서 match_user를 선점하게되어 이미 게임 시작 중인 유저가 다시 매칭될수 있다.

        }

        async Task<ServerCommon.Channel> GetChannel(long match_id)
        {
            var db = Cache.Instance.GetDatabase();
            var match_value = await db.StringGetAsync(string.Format("match:{0}", match_id));
            if (match_value.HasValue)
            {
                var channel_value = await db.HashGetAsync("channel_info", match_value);
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


        public override async Task StartPlay(StartPlayRequest request, IServerStreamWriter<StartPlayReply> responseStream, ServerCallContext context)
        {
            var session = await Session.GetSession(request.SessionId);
            if (session == null)
            {
                Log.Warning("lost session");
                await responseStream.WriteAsync(new StartPlayReply()
                {
                    Code = ErrorCode.LostSession
                });
                return;
            }


            var db = Cache.Instance.GetDatabase();

            // 다른 유저가 매칭 시도를 했을 경우 
            var value = await db.StringGetAsync(string.Format("match_user:{0}", session.user_no));
            if (value.HasValue)
            {
                // 매칭 시도가 성공했을 경우
                var ch = await GetChannel((long)value);
                if (ch != null)
                {
                    await RemoveMatchUser(session.user_no);


                    Log.Information(string.Format("StartPlay {0}", session.user_no));

                    await responseStream.WriteAsync(new StartPlayReply()
                    {
                        Code = ErrorCode.Success,
                        IsStart = true,
                        BattleServerAddr = ch.server_addr,
                        WorldId = ch.world_id,
                    });
                    return;
                }
                else
                {
                    //db.KeyDelete(string.Format("match_user:{0}", session.user_no));
                    await responseStream.WriteAsync(new StartPlayReply()
                    {
                        Code = ErrorCode.BusyServer,
                    });
                    return;
                }
            }

            long match_id = 0;
            var player_list = new List<long>();
            var waiting_list = await db.SortedSetRangeByScoreAsync("waiting_list");
            if (waiting_list.Length > MAX_START_PLAYER_COUNT - 1)
            {
                match_id = await db.StringIncrementAsync("match_instance_id");
                for (int i = 0; i < waiting_list.Length; ++i)
                {
                    long user_no = (long)waiting_list[i];
                    // 자신은 스킵
                    if (user_no == session.user_no)
                        continue;

                    if ((await db.StringGetAsync(string.Format("waiting:{0}", user_no))).HasValue)
                    {
                        // 매칭에 필요한 유저를 선점한다
                        if (await db.StringSetAsync(string.Format("match_user:{0}", user_no), match_id, match_user_expire, When.NotExists) == true)
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
                        await RemoveMatchUser(user_no);
                    }
                }
            }

            if (player_list.Count == MAX_START_PLAYER_COUNT - 1)
            {
                // 매칭에 필요한 인원을 모두 찾았을때
                // 전투 가능한 서버를 찾아 세팅
                (bool ret, string server_addr, byte worldId, string channel_key) = await GetAvailableServer();
                if (ret)
                {
                    var reply = new StartPlayReply()
                    {
                        Code = ErrorCode.Success,
                        IsStart = true,
                        BattleServerAddr = server_addr,
                        WorldId = worldId,
                    };


                    // 매칭된 유저들에게 알림
                    for (int i=0;i<player_list.Count;++i)
                    {
                        await Cache.Instance.GetSubscriber().PublishAsync(string.Format("sub_user:{0}", player_list[i]), json_formatter.Format(reply));
                    }

                    await db.StringSetAsync(string.Format("match:{0}", match_id), channel_key, match_expire);

                    await RemoveMatchUser(session.user_no);


                    Log.Information(string.Format("StartPlay {0}", session.user_no));

                    await responseStream.WriteAsync(reply);
                    return;
                }
                else
                {
                    // 전투 가능한 서버가 없다
                    Log.Information(string.Format("Cannot find Server user_no:{0}", session.user_no));
                    await responseStream.WriteAsync(new StartPlayReply()
                    {
                        Code = ErrorCode.BusyServer,
                    });
                    return;
                }
            }


            // 매칭 시도를 실패하면 선점한 유저를 삭제
            await db.KeyDeleteAsync(player_list.Select(key => (RedisKey)string.Format("match_user:{0}", key)).ToArray());


            // 대기자로 등록
            await db.SortedSetAddAsync("waiting_list", session.user_no, session.rating);
            await db.StringSetAsync(string.Format("waiting:{0}", session.user_no), 0, startplay_polling_period);


            // 조건에 만족하는 유저가 없다면 대기 (redis puh로 활성화)
            var queue = Cache.Instance.GetSubscriber().Subscribe(string.Format("sub_user:{0}", session.user_no));

            var cts = new CancellationTokenSource();
            cts.CancelAfter(15 * 1000);
            try
            {
                var ret = await queue.ReadAsync(cts.Token);
                // 다른 유저로 부터 매칭이 되었음을 받았다
                // 매칭 정보를 삭제
                await RemoveMatchUser(session.user_no);

                // 매칭이 성공되었음을 알림
                StartPlayReply reply = JsonParser.Default.Parse<StartPlayReply>(ret.Message);
                await responseStream.WriteAsync(reply);
            }
            catch (OperationCanceledException)
            {
                // 대기시간 만료 클라이언트에게 타임 아웃 처리를 보낸다.
                await responseStream.WriteAsync(new StartPlayReply()
                {
                    Code = ErrorCode.Timeout,
                    IsStart = false
                });

            }
        }
    }
}
