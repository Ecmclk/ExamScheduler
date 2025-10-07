using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Models.Models;

[Table("Roller")]
[Index("RolAd", Name = "UQ__Roller__F92343FA036F3BB5", IsUnique = true)]
public partial class Roller
{
    [Key]
    [Column("RolID")]
    public int RolId { get; set; }

    [StringLength(40)]
    public string RolAd { get; set; } = null!;

    [InverseProperty("Rol")]
    public virtual ICollection<Kullanicilar> Kullanicilars { get; set; } = new List<Kullanicilar>();
}
