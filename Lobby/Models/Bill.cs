using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public string receipt { get; set; }

        public DateTime submit_date { get; set; }
    }
}
