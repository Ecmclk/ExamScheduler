using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;


namespace ExamScheduler.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        // Constructor: Bu servis oluşturulduğunda, ona veritabanı bağlamını (AppDbContext) veriyoruz.
        public UserService(AppDbContext context)
        {
            _context = context;
        }

        /*Verilen e-posta ve şifre ile kullanıcı girişini doğrular.
          name="email">Kullanıcının giriş yaptığı e-posta.
          name="password">Kullanıcının girdiği düz metin şifre
          Giriş başarılı ise Kullanicilar nesnesini, değilse null döner. */
        public Kullanicilar? ValidateUser(string email, string password)
        {
            // 1. Adım: E-postaya göre kullanıcıyı veritabanında bul.
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Eposta == email);

            // 2. Adım: Kullanıcı bulunamadıysa veya şifre yanlışsa null dön.
            if (user == null || !VerifyPasswordHash(password, user.SifreHash, user.SifreSalt))
            {
                return null; // Kullanıcı yok veya şifre hatalı.
            }

            // 3. Adım: Kullanıcı bulundu ve şifre doğruysa, kullanıcı nesnesini geri döndür.
            return user;
        }

        /// Girilen bir şifrenin, veritabanındaki hash ve salt ile eşleşip eşleşmediğini kontrol eder.
        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Veritabanındaki salt'ı kullanarak girilen şifrenin hash'ini yeniden hesapla.
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Hesaplanan hash ile veritabanındaki hash'in birebir aynı olup olmadığını kontrol et.
                return computedHash.SequenceEqual(storedHash);
            }
        }

        /// Verilen bir şifre için Hash ve Salt değerleri oluşturur. (Yeni kullanıcı oluştururken kullanılır)
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; // Rastgele güvenli bir anahtar (salt) oluşturulur.
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Salt kullanılarak hash hesaplanır.
            }
        }
    }
}