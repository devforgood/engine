using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lobby
{
    public class ServerConfiguration
    {
        /// <summary>
        /// Get global instance of Cache
        /// </summary>
        public static readonly ServerConfiguration Instance = new ServerConfiguration();

        public IConfiguration config;

        private ServerConfiguration()
        {
            config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
        }
    }
}
