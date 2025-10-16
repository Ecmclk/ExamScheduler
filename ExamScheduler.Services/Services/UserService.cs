using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


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
        public List<Kullanicilar> GetAllUsers()
        {
            // .Include() metotları, Entity Framework'e Kullanicilar tablosunu çekerken,
            // onlara bağlı olan Rol ve Bolum tablolarındaki verileri de getirmesini söyler.
            return _context.Kullanicilar
                           .Include(u => u.Rol)
                           .Include(u => u.Bolum)
                           .ToList();
        }
        public bool AddUser(string adSoyad, string eposta, string sifre, int rolId, int bolumId)
        {
            // 1. Kontrol: Bu e-posta ile başka bir kullanıcı var mı?
            if (_context.Kullanicilar.Any(u => u.Eposta == eposta))
            {
                return false; // Varsa, işlem başarısız.
            }

            // 2. Şifreyi hash'le.
            CreatePasswordHash(sifre, out byte[] passwordHash, out byte[] passwordSalt);

            // 3. Yeni kullanıcı nesnesini oluştur.
            var newUser = new Kullanicilar
            {
                AdSoyad = adSoyad,
                Eposta = eposta,
                SifreHash = passwordHash,
                SifreSalt = passwordSalt,
                RolId = rolId,
                BolumId = bolumId,
                Aktiflik = true // Yeni kullanıcı varsayılan olarak aktif olsun.
            };

            // 4. Veritabanına ekle ve kaydet.
            _context.Kullanicilar.Add(newUser);
            _context.SaveChanges();

            return true; // İşlem başarılı.
        }
        public bool DeleteUser(int kullaniciId)
        {
            // 1. Silinecek kullanıcıyı ID'sine göre veritabanında bul.
            var userToDelete = _context.Kullanicilar.FirstOrDefault(u => u.KullaniciId == kullaniciId);

            // 2. Kullanıcı bulunamazsa, işlemi durdur ve başarısız olarak dön.
            if (userToDelete == null)
            {
                return false;
            }

            // 3. Admin kullanıcısının (RolId = 1) silinmesini engelle.
            // Bu, sistemin yöneticisiz kalmasını önler.
            if (userToDelete.RolId == 1)
            {
                // İsteğe bağlı: Burada bir hata fırlatabilir veya sadece false dönebiliriz.
                return false;
            }

            // 4. Kullanıcıyı veritabanından sil ve değişiklikleri kaydet.
            _context.Kullanicilar.Remove(userToDelete);
            _context.SaveChanges();

            return true; // İşlem başarılı.
        }

    }
}