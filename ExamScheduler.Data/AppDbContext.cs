using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
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
                // Mevcut veritabanına bağlanacak bağlantı
                optionsBuilder.UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=SinavPlanlayiciDB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Burayı şimdilik boş bırakabilirsin
        }
    }
}
