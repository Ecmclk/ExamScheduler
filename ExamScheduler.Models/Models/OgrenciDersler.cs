
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamScheduler.Models.Models
{
    [Table("OgrenciDersler")]
    public class OgrenciDersler
    {
        [Key]
        public int Id { get; set; }

        [Column("OgrenciID")]
        public int OgrenciId { get; set; }

        [Column("DersID")]
        public int DersId { get; set; }

        // Navigation Properties
        [ForeignKey("OgrenciId")]
        public virtual Ogrenciler Ogrenci { get; set; } = null!;

        [ForeignKey("DersId")]
        public virtual Dersler Ders { get; set; } = null!;
    }
}
