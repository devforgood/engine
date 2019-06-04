using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Subscribe
    {
        public static void Do()
        {
            ISubscriber sub = ServerMonitor.sInstance.cache.GetSubscriber();
            sub.Subscribe("messages", (channel, message) =>
            {
                Log.Information(string.Format("redis msg {0}", message));
                var svr = NetworkManagerServer.sInstance.GetServer();
                var msg = svr.CreateMessage();
                msg.Write((string)message);
                svr.SendUnconnectedToSelf(msg, true);
            });
        }

    }
}
