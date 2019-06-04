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
            var world_count = Convert.ToByte(config["world_count"]);
            var server_addr = ip + ":" + port;

            var redis_ip = config["redis:ip"];
            var redis_port = config["redis:port"];
            var redis_addr = redis_ip + ":" + redis_port;


            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs\\battle.txt", rollingInterval: RollingInterval.Day
                    , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} ({ThreadId}) [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting Battle Server host");
                if (Server.StaticInit(port, world_count))
                {
                    ServerMonitor.sInstance.Init(world_count, server_addr, redis_addr, server_name);
                    Subscribe.Do();
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
