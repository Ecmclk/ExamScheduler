using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamScheduler.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Bolumler> Bolumler { get; set; }
        public DbSet<Dersler> Dersler { get; set; }
        public DbSet<Derslikler> Derslikler { get; set; }
        public DbSet<Kullanicilar> Kullanicilar { get; set; }
        public DbSet<Ogrenciler> Ogrenciler { get; set; }
        public DbSet<Roller> Roller { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = config.GetConnectionString("SinavPlanlayiciDB");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Burayı şimdilik boş bırakabilirsin
        }
    }
}
