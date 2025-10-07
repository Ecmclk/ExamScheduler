using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Dersler")]
[Index("DersKodu", Name = "UQ__Dersler__9DCB30EF802CD853", IsUnique = true)]
public partial class Dersler
{
    [Key]
    [Column("DersID")]
    public int DersId { get; set; }

    [Column("BolumID")]
    public int BolumId { get; set; }

    [StringLength(20)]
    public string DersKodu { get; set; } = null!;

    [StringLength(150)]
    public string DersAdi { get; set; } = null!;

    [StringLength(120)]
    public string OgretimGorevlisi { get; set; } = null!;

    public byte Sinif { get; set; }

    [StringLength(20)]
    public string DersTuru { get; set; } = null!;

    [ForeignKey("BolumId")]
    [InverseProperty("Derslers")]
    public virtual Bolumler Bolum { get; set; } = null!;
}
