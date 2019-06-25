using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lobby.Models
{
    public class CommonContext : DbContext
    {
        public CommonContext(DbContextOptions<CommonContext> options)
            : base(options)
        {
        }

        public DbSet<Lobby.Models.User> User { get; set; }
    }
}
