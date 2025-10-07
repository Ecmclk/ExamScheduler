using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bolumler> Bolumlers { get; set; }

    public virtual DbSet<Dersler> Derslers { get; set; }

    public virtual DbSet<Derslikler> Dersliklers { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Ogrenciler> Ogrencilers { get; set; }

    public virtual DbSet<Roller> Rollers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SinavPlanlayiciDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bolumler>(entity =>
        {
            entity.HasKey(e => e.BolumId).HasName("PK__Bolumler__7BAD4B5C81E97772");
        });

        modelBuilder.Entity<Dersler>(entity =>
        {
            entity.HasKey(e => e.DersId).HasName("PK__Dersler__E8B3DE71747C647D");

            entity.HasOne(d => d.Bolum).WithMany(p => p.Derslers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dersler_Bolumler");
        });

        modelBuilder.Entity<Derslikler>(entity =>
        {
            entity.HasKey(e => e.DerslikId).HasName("PK__Derslikl__8E733DCAD895FF45");

            entity.HasOne(d => d.Bolum).WithMany(p => p.Dersliklers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Derslikler_Bolumler");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("PK__Kullanic__E011F09BCAD6DEF9");

            entity.Property(e => e.Aktiflik).HasDefaultValue(true);

            entity.HasOne(d => d.Bolum).WithMany(p => p.Kullanicilars).HasConstraintName("FK_Kullanicilar_Bolumler");

            entity.HasOne(d => d.Rol).WithMany(p => p.Kullanicilars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Kullanicilar_Roller");
        });

        modelBuilder.Entity<Ogrenciler>(entity =>
        {
            entity.HasKey(e => e.OgrenciId).HasName("PK__Ogrencil__E497E6D45A2531FA");

            entity.HasOne(d => d.Bolum).WithMany(p => p.Ogrencilers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ogrenciler_Bolumler");
        });

        modelBuilder.Entity<Roller>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roller__F92302D1E100B2BD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
