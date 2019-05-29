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

        public IActionResult StartPlay()
        {
            var session = this.GetSession();
            if(session == null)
            {
                Log.Warning("lost session");
                return null;
            }

            var db = Cache.Instance.GetDatabase();

            // 대기자 목록 검색
            var waiting_list = db.SortedSetRangeByScore("waiting_list");
            for (int i = 0; i < waiting_list.Length; ++i)
            {

            }


            // 대기자로 등록
            db.SortedSetAdd("waiting_list", "test1", 1);

            // 시작 조건 만족시 게임 시작


            return null;
        }
    }
}