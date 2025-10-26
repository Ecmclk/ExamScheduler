using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamScheduler.Models.Models
{
    public static class AktifKullanici
    {
        public static int KullaniciID { get; set; }
        public static string AdSoyad { get; set; } = string.Empty;
        public static int RolID { get; set; }
        public static int? BolumID { get; set; }
        public static string Eposta { get; set; } = string.Empty;
    }
}
