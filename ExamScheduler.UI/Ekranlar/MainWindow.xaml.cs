using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using ExamScheduler.UI.Ekranlar;
using System.Windows;

namespace ExamScheduler.UI
{
    public partial class MainWindow : Window
    {
        // Artık AppDbContext'e doğrudan ihtiyacımız yok, sadece servise ihtiyacımız var.
        private readonly UserService _userService;

        // Constructor'ı, bir UserService alacak şekilde güncelliyoruz.
        public MainWindow(UserService userService)
        {
            InitializeComponent();

            // Dışarıdan gelen servisi kendi değişkenimize atıyoruz.
            // Artık 'new' ile yeniden oluşturmuyoruz!
            _userService = userService;
        }

        private void GirisYapButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EpostaTextBox.Text;
            string password = SifrePasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("E-posta ve şifre alanları boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Kullanicilar? user = _userService.ValidateUser(email, password);

            if (user != null)
            {
                MainPanelWindow mainPanel = new MainPanelWindow(user);
                mainPanel.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Hatalı e-posta veya şifre. Lütfen tekrar deneyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}