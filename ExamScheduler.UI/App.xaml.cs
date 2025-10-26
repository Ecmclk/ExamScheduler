using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows;

namespace ExamScheduler.UI
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new AppDbContext();
            var userService = new UserService(context);

            await SeedAdminUserAsync(context, userService);

            // MainWindow parametresiz versiyon
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private async Task SeedAdminUserAsync(AppDbContext context, UserService userService)
        {
            // Eğer hiç kullanıcı yoksa varsayılan admin ekle
            if (!await context.Kullanicilar.AnyAsync())
            {
                userService.CreatePasswordHash("admin123", out byte[] hash, out byte[] salt);

                var admin = new Kullanicilar
                {
                    Eposta = "admin@example.com",
                    AdSoyad = "Sistem Yöneticisi",
                    RolId = 1,           // 1 = Admin
                    BolumId = null,      // Admin bölümsüz
                    Aktiflik = true,
                    SifreHash = hash,
                    SifreSalt = salt
                };

                context.Kullanicilar.Add(admin);
                await context.SaveChangesAsync();

            }
        }
    }
}
