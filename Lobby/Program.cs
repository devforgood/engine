using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;

namespace Lobby
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
                .WriteTo.File("logs\\lobby.txt", rollingInterval: RollingInterval.Day
                    , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} ({ThreadId}) [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Server server = new Server
            {
                Services = { GameService.Lobby.BindService(new LobbyService()) },
                Ports = { new ServerPort("0.0.0.0", port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
