using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MasterCsharpHosted.Server.Data
{
    public class AppUserContext : DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }

        public AppUserContext(DbContextOptions<AppUserContext> options) : base(options) { }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseCosmos(Configuration.GetValue<string>("Cosmos:Endpoint"),
        //        Configuration.GetValue<string>("Cosmos:Key"), databaseName: "AppUserDb");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().ToContainer("AppUsers");
            modelBuilder.Entity<AppUser>().HasPartitionKey(a => a.UserName);
            modelBuilder.Entity<AppUser>().OwnsMany(a => a.Snippets);
            modelBuilder.Entity<AppUser>().OwnsMany(a => a.CompletedChallenges);
        }
    }
}
