using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using System.Linq;
using System.Windows;

namespace ExamScheduler.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Gerekli servisleri ve veritabanı bağlamını oluşturuyoruz.
            AppDbContext context = new AppDbContext();
            UserService userService = new UserService(context);

            // Admin kullanıcısını oluşturma metodunu burada çağırıyoruz.
            SeedAdminUser(context, userService);

            // Artık ana penceremizi oluşturup gösterebiliriz.
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        /// <summary>
        /// Veritabanını kontrol eder ve hiç kullanıcı yoksa varsayılan Admin kullanıcısını oluşturur.
        /// </summary>
        private void SeedAdminUser(AppDbContext context, UserService userService)
        {
            // Eğer veritabanındaki Kullanicilar tablosunda HİÇ kayıt yoksa
            if (!context.Kullanicilar.Any())
            {
                // Admin için bir şifre belirliyoruz ve bunun hash/salt değerlerini üretiyoruz.
                userService.CreatePasswordHash("admin123", out byte[] passwordHash, out byte[] passwordSalt);

                // Yeni bir Kullanicilar nesnesi oluşturup bilgilerini dolduruyoruz.
                var adminUser = new Kullanicilar
                {
                    Eposta = "admin@example.com",
                    AdSoyad = "Sistem Yöneticisi",
                    RolId = 1, // Roller tablosunda 'Admin' ID'sinin 1 olduğunu varsayıyoruz.
                    Aktiflik = true,
                    BolumId = null, // Admin bir bölüme ait değildir.
                    SifreHash = passwordHash,
                    SifreSalt = passwordSalt
                };

                // Oluşturduğumuz bu kullanıcıyı veritabanına ekliyoruz.
                context.Kullanicilar.Add(adminUser);
                context.SaveChanges(); // Değişiklikleri veritabanına kaydediyoruz.
                MessageBox.Show("Veritabanı boştu. Varsayılan Admin kullanıcısı oluşturuldu.\nE-posta: admin@example.com\nŞifre: admin123", "Kurulum Bilgisi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
