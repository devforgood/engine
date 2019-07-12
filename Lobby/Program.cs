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
            var port = Convert.ToUInt16(ServerConfiguration.Instance.config["port"]);


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
