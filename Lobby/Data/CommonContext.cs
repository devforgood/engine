using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lobby.Models
{
    public class CommonContext : DbContext
    {
        public DbSet<Lobby.Models.User> User { get; set; }
        public DbSet<Lobby.Models.Bill> Bill { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ServerConfiguration.Instance.CommonContext);
        }

    }
}
