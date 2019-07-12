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

        string redis_server_addr = ServerConfiguration.Instance.config["redis:ip"] + ":" + ServerConfiguration.Instance.config["redis:port"];
        string redis_server_ip = ServerConfiguration.Instance.config["redis:ip"];
        UInt16 redis_server_port = UInt16.Parse(ServerConfiguration.Instance.config["redis:port"]);

        public ConnectionMultiplexer redis;


        private Cache()
        {
            redis = ConnectionMultiplexer.Connect(redis_server_addr);
        }


        ~Cache()
        {
            redis.Dispose();
        }

        public ConnectionMultiplexer GetConnection()
        {
            return redis;
        }

        public ISubscriber GetSubscriber()
        {
            return redis.GetSubscriber();
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
