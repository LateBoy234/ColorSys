using ColorSys.Domain.Config;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.DbHandle
{
    public  class ConfigDbContext: DbContext
    {
        public DbSet<SystemConfig> Configs => Set<SystemConfig>();
        public ConfigDbContext(DbContextOptions<ConfigDbContext> opt) : base(opt) { }
        protected override void OnModelCreating(ModelBuilder b) =>
            b.Entity<SystemConfig>().HasIndex(x => x.Key).IsUnique();
    }
}
