using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class CacheThread
    {
        public static void Run()
        {
            Task.Run(() =>
            {
                while(Server.sInstance.IsRunning)
                {
                    ServerMonitor.sInstance.Update();
                    Thread.Sleep(10000);
                }
            });
        }
    }
}
