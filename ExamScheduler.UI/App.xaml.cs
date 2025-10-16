// App.xaml.cs dosyasının YENİ HALİ

using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Services;
using System.Linq;
using System.Windows;

namespace ExamScheduler.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDbContext context = new AppDbContext();
            UserService userService = new UserService(context);

            // Admin kullanıcısını oluşturma metodunu çağırıyoruz.
            SeedAdminUser(context, userService);

            // Ana penceremizi oluştururken, hazırladığımız userService'i ona veriyoruz.
            MainWindow mainWindow = new MainWindow(userService);
            mainWindow.Show();
        }

        private void SeedAdminUser(AppDbContext context, UserService userService)
        {
            // Bu metodun içeriği aynı kalıyor...
        }
    }
}