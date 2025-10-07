using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Ogrenciler")]
[Index("OgrenciNo", Name = "UQ__Ogrencil__E497FE1A0D69C5B3", IsUnique = true)]
public partial class Ogrenciler
{
    [Key]
    [Column("OgrenciID")]
    public int OgrenciId { get; set; }

    [StringLength(20)]
    public string OgrenciNo { get; set; } = null!;

    [StringLength(120)]
    public string AdSoyad { get; set; } = null!;

    [Column("BolumID")]
    public int BolumId { get; set; }

    public byte Sinif { get; set; }

    [ForeignKey("BolumId")]
    [InverseProperty("Ogrencilers")]
    public virtual Bolumler Bolum { get; set; } = null!;
}
