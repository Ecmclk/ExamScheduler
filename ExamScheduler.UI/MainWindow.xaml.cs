using System.Windows;
using ExamScheduler.Data;
using ExamScheduler.Services;
using ExamScheduler.Models.Models;

namespace ExamScheduler.UI
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public MainWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _userService = new UserService(_context);
        }
        

        private void GirisButton_Click(object sender, RoutedEventArgs e)
        {
            string eposta = EpostaTextBox.Text.Trim();
            string sifre = SifreBox.Password.Trim();

            if (string.IsNullOrEmpty(eposta) || string.IsNullOrEmpty(sifre))
            {
                UyariText.Text = "E-posta ve şifre alanları boş olamaz.";
                UyariBorder.Visibility = Visibility.Visible;
                return;
            }

            var user = _userService.ValidateUser(eposta, sifre);

            if (user == null)
            {
                UyariText.Text = "Geçersiz e-posta veya şifre.";
                UyariBorder.Visibility = Visibility.Visible;
                return;
            }

            // Eğer kullanıcı aktif değilse girişe izin verme
            if (!user.Aktiflik)
            {
                UyariText.Text = "Bu kullanıcı hesabı pasif durumda.";
                UyariBorder.Visibility = Visibility.Visible;
                return;
            }
            
            // Örnek: login sonrası
            var girisYapanKullanici = _context.Kullanicilar.FirstOrDefault(k => k.Eposta == EpostaTextBox.Text);

// Kullanıcının rolünü ve bölümünü al
            int rol = girisYapanKullanici.RolId;
            int? bolumId = girisYapanKullanici.BolumId;

            // Başarılı giriş
            var anaPencere = new MainPanelWindow(rol, bolumId);
            Application.Current.MainWindow = anaPencere; // 🔽 Burası çok önemli
            anaPencere.Show();
            
            this.Close();

        }
        
        
        private void TestUserEkleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userService = new UserService(_context);

                bool added = userService.AddUser(
                    adSoyad: "Admin",
                    eposta: "admin@mail.com",
                    sifre: "1",
                    rolId: 1,
                    bolumId: 1
                );

                MessageBox.Show(added ? "Test kullanıcı eklendi." : "Kullanıcı zaten mevcut.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        

    }
}