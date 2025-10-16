using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class MainPanelWindow : Window
    {
        private readonly Kullanicilar _currentUser;

        public MainPanelWindow(Kullanicilar user)
        {
            InitializeComponent();
            _currentUser = user;

            // Panelin kime ait olduğunu göstermek için kullanıcı bilgilerini yazdır.
            KullaniciAdSoyadText.Text = _currentUser.AdSoyad;
            KullaniciRolText.Text = _currentUser.Rol.RolAd;
            WelcomeMessageText.Text = $"Hoş geldiniz, {_currentUser.AdSoyad}";

            // Uygulama açıldığında varsayılan olarak Kullanıcı Yönetimi'ni göster.
            MainContentArea.Content = new KullaniciYonetimi();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button? clickedButton = sender as Button;
            if (clickedButton == null) return;

            switch (clickedButton.Name)
            {
                case "KontrolPaneliMenuButton":
                    // TODO: MainContentArea.Content = new KontrolPaneli();
                    break;
                case "KullanicilarMenuButton":
                    MainContentArea.Content = new KullaniciYonetimi();
                    break;
                case "BolumlerMenuButton":
                    // TODO: MainContentArea.Content = new BolumYonetimi();
                    break;
            }
        }

        private void CikisButton_Click(object sender, RoutedEventArgs e)
        {
            // Çıkış yapıldığında, giriş ekranını yeniden oluştururken
            // ona gerekli olan UserService'i de vermeliyiz.
            AppDbContext context = new AppDbContext();
            UserService userService = new UserService(context);
            MainWindow loginWindow = new MainWindow(userService);

            loginWindow.Show();
            this.Close();
        }
    }
}