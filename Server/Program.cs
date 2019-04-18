using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var log4net_config_dir = string.Format("{0}\\{1}", System.IO.Directory.GetCurrentDirectory(), "LogConfig");
            if ('\\' != log4net_config_dir.Last())
            {
                log4net_config_dir = string.Format("{0}\\log4net.xml", log4net_config_dir);
            }
            else
            {
                log4net_config_dir = string.Format("{0}log4net.xml", log4net_config_dir);
            }

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(log4net_config_dir));

            log.Info("INFO 출력");
            if (Server.StaticInit(65000))
            {
                Server.sInstance.Run();
            }
        }
    }
}
