using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Derslikler")]
[Index("DerslikKod", Name = "UQ__Derslikl__041E03CAADCAED8B", IsUnique = true)]
[Index("DerslikAd", Name = "UQ__Derslikl__8E73FCE1D6CDC992", IsUnique = true)]
public partial class Derslikler
{
    [Key]
    [Column("DerslikID")]
    public int DerslikId { get; set; }

    [Column("BolumID")]
    public int BolumId { get; set; }

    [StringLength(20)]
    public string DerslikKod { get; set; } = null!;

    [StringLength(100)]
    public string DerslikAd { get; set; } = null!;

    public int Kapasite { get; set; }

    public int EnineSira { get; set; }

    public int BoyunaSira { get; set; }

    public byte SiraYapisi { get; set; }

    [ForeignKey("BolumId")]
    [InverseProperty("Dersliklers")]
    public virtual Bolumler Bolum { get; set; } = null!;
}
