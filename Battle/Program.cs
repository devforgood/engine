using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var server_name = config["name"];
            var ip = config["ip"];
            var port = Convert.ToUInt16(config["port"]);



            var redis_ip = config["redis:ip"];
            var redis_port = config["redis:port"];


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs\\battle.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting Battle Server host");
                if (Server.StaticInit(port))
                {
                    Server svr = (Server)(Server.sInstance);
                    svr.server_info.server_name = server_name;
                    svr.server_info.server_addr = ip + ":" + port;

                    // connect redis
                    var redis_addr = redis_ip + ":" + redis_port;
                    svr.redis = ConnectionMultiplexer.Connect(redis_addr);

                    var db = svr.redis.GetDatabase();
                    svr.server_info.server_id = server_name + ":" + Convert.ToString(db.StringIncrement(ServerCommon.ServerInfoRedisKey.server_instance_id));

                    db.HashSet("server_info", svr.server_info.server_addr, JsonConvert.SerializeObject(svr.server_info));


                    Server.sInstance.Run();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}
