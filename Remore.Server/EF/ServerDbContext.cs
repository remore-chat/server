using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.Library.Models;
using Remore.Server.Services;

namespace Remore.Server.EF
{
    public class ServerDbContext : DbContext
    {
        public ServerDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ServerConfiguration> Configuration { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelMessage> ChannelMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=server_database.db");
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif
        }
    }
}
