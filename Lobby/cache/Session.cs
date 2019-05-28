using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby
{
    public class Session
    {
        Guid session_id;
        string remote_endpoint;
        long user_no;
        public static TimeSpan session_expire = new TimeSpan(0, 5, 0);

        public Session()
        {
            session_id = Guid.NewGuid();
        }

        public static Session CreateSession(string endpoint)
        {
            var session = new Session();
            session.remote_endpoint = endpoint;
            var db = Cache.Instance.GetDatabase();

            // todo : 디비에 저장된 user_no 로 변경 예정
            session.user_no = db.StringIncrement("temp_user_no");

            db.StringSet(string.Format("session:{0}", session.session_id.ToString()), JsonConvert.SerializeObject(session), session_expire);

            return session;
        }

        public static Session GetSession(string session_id)
        {
            var db = Cache.Instance.GetDatabase();
            var key = string.Format("session:{0}", session_id);
            var ret = db.StringGet(key);
            if (ret.HasValue == false)
                return null;

            db.KeyExpire(key, session_expire);

            var session = JsonConvert.DeserializeObject<Session>(ret);

            return session;
        }

    }
}
