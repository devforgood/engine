using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby.Models
{
    public class User
    {
        public int Id { get; set; }
        public string name { get; set; }

        public string nation { get; set; }

        public DateTime last_play_time { get; set; }
    }
}
