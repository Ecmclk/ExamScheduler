using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Bolumler> Bolumler { get; set; } = null!;
        public DbSet<Dersler> Dersler { get; set; } = null!;
        public DbSet<Derslikler> Derslikler { get; set; } = null!;
        public DbSet<Kullanicilar> Kullanicilar { get; set; } = null!;
        public DbSet<Ogrenciler> Ogrenciler { get; set; } = null!;
        public DbSet<OgrenciDersler> OgrenciDersler { get; set; } = null!;
        public DbSet<Roller> Roller { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SinavPlanlayiciDB;Trusted_Connection=True;");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OgrenciDersler>()
                .HasOne(od => od.Ogrenci)
                .WithMany(o => o.OgrenciDersler)
                .HasForeignKey(od => od.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict); // burada cascade iptal

            modelBuilder.Entity<OgrenciDersler>()
                .HasOne(od => od.Ders)
                .WithMany(d => d.OgrenciDersler)
                .HasForeignKey(od => od.DersId)
                .OnDelete(DeleteBehavior.Cascade); // diğerinde bırakabilirsin
        }
    }
}
