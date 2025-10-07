using ExamScheduler.Models.Models;
using System;
namespace ExamScheduler
{
     class Test
    {
        public static void TestConnection()
        {
            try
            {
                using var context = new AppDbContext();

                // Users tablosu scaffold edilmişse: Kullanicilars
                var userCount = context.Kullanicilars.Count();

                Console.WriteLine($"Veritabanına bağlanıldı! Kullanıcı sayısı: {userCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bağlantı hatası: " + ex.Message);
            }
        }
    }
}