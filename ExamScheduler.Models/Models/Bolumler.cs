using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Bolumler")]
[Index("BolumKod", Name = "UQ__Bolumler__4BC8DE108A0D08B3", IsUnique = true)]
public partial class Bolumler
{
    [Key]
    [Column("BolumID")]
    public int BolumId { get; set; }

    [StringLength(10)]
    public string BolumKod { get; set; } = null!;

    [StringLength(120)]
    public string BolumAd { get; set; } = null!;

    [InverseProperty("Bolum")]
    public virtual ICollection<Dersler> Derslers { get; set; } = new List<Dersler>();

    [InverseProperty("Bolum")]
    public virtual ICollection<Derslikler> Dersliklers { get; set; } = new List<Derslikler>();

    [InverseProperty("Bolum")]
    public virtual ICollection<Kullanicilar> Kullanicilars { get; set; } = new List<Kullanicilar>();

    [InverseProperty("Bolum")]
    public virtual ICollection<Ogrenciler> Ogrencilers { get; set; } = new List<Ogrenciler>();
}
