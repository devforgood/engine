using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby
{
    public class Session : core.Session
    {
        private static TimeSpan session_expire = new TimeSpan(0, 5, 0);

        public Session()
        {
            session_id = Guid.NewGuid().ToString();
        }

        public static async Task<Session> CreateSession(string endpoint)
        {
            var session = new Session();
            session.remote_endpoint = endpoint;
            var db = Cache.Instance.GetDatabase();

            // todo : 디비에 저장된 user_no 로 변경 예정
            session.user_no = await db.StringIncrementAsync("temp_user_no");

            await db.StringSetAsync(string.Format("session:{0}", session.session_id.ToString()), JsonConvert.SerializeObject(session), session_expire);
            await db.StringSetAsync(string.Format("user:{0}", session.user_no), session.session_id.ToString(), session_expire);

            return session;
        }

        public static async Task<Session> GetSession(string session_id, bool touch = true)
        {
            var db = Cache.Instance.GetDatabase();
            var key = string.Format("session:{0}", session_id);
            var ret = await db.StringGetAsync(key);
            if (ret.HasValue == false)
                return null;

            var session = JsonConvert.DeserializeObject<Session>(ret);

            if (touch == true)
            {
                await db.KeyExpireAsync(key, session_expire);
                await db.KeyExpireAsync(string.Format("user:{0}", session.user_no), session_expire);
            }

            return session;
        }

        public static async Task<bool> IsAvailableSesssion(long user_no)
        {
            return (await Cache.Instance.GetDatabase().StringGetAsync(string.Format("user:{0}", user_no))).HasValue;

        }

    }
}
