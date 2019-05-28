using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby
{
    public class Cache
    {
        /// <summary>
        /// Get global instance of Cache
        /// </summary>
        public static readonly Cache Instance = new Cache();

        string redis_server_addr => string.Format("{0}:{1}", Startup.StaticConfig["redis:ip"], Startup.StaticConfig["redis:port"]);
        string redis_server_ip => Startup.StaticConfig["redis:ip"];
        UInt16 redis_server_port => Convert.ToUInt16(Startup.StaticConfig["redis:port"]);

        public ConnectionMultiplexer redis;


        private Cache()
        {
            redis = ConnectionMultiplexer.Connect(redis_server_addr);
        }


        ~Cache()
        {
            redis.Dispose();
        }


        public IDatabase GetDatabase()
        {
            return redis.GetDatabase();
        }

        public IServer GetServer()
        {
            return redis.GetServer(redis_server_ip, redis_server_port);
        }
    }
}
