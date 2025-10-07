using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Kullanicilar")]
[Index("Eposta", Name = "UQ__Kullanic__03ABA3916F7CD1B2", IsUnique = true)]
public partial class Kullanicilar
{
    [Key]
    [Column("KullaniciID")]
    public int KullaniciId { get; set; }

    [StringLength(150)]
    public string Eposta { get; set; } = null!;

    [MaxLength(64)]
    public byte[] SifreHash { get; set; } = null!;

    [MaxLength(32)]
    public byte[] SifreSalt { get; set; } = null!;

    [StringLength(120)]
    public string AdSoyad { get; set; } = null!;

    [Column("RolID")]
    public int RolId { get; set; }

    [Column("BolumID")]
    public int? BolumId { get; set; }

    public bool Aktiflik { get; set; }

    [ForeignKey("BolumId")]
    [InverseProperty("Kullanicilars")]
    public virtual Bolumler? Bolum { get; set; }

    [ForeignKey("RolId")]
    [InverseProperty("Kullanicilars")]
    public virtual Roller Rol { get; set; } = null!;
}
