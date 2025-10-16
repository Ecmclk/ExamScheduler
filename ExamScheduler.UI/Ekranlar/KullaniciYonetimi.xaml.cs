using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class KullaniciYonetimi : UserControl
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public KullaniciYonetimi()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _userService = new UserService(_context);

            // Ekran yüklendiğinde hem Bölüm listesini hem de Kullanıcı listesini doldur.
            LoadBolumler();
            LoadUsers();
        }

        // ComboBox'ı (açılır listeyi) veritabanındaki bölümlerle dolduran metot.
        private void LoadBolumler()
        {
            var bolumler = _context.Bolumler.ToList();
            BolumComboBox.ItemsSource = bolumler;
            BolumComboBox.DisplayMemberPath = "BolumAd"; // Kullanıcının göreceği metin (örn: "Bilgisayar Mühendisliği")
            BolumComboBox.SelectedValuePath = "BolumID"; // Arka planda tutulacak değer (örn: 1)
        }

        // Veritabanından gerçek kullanıcıları yükleyen metot.
        private void LoadUsers()
        {
            var usersFromDb = _userService.GetAllUsers();

            var usersForGrid = usersFromDb.Select(user => new UserViewModel
            {
                Id = user.KullaniciId,
                AdSoyad = user.AdSoyad,
                Eposta = user.Eposta,
                Rol = user.Rol.RolAd,
                Bolum = user.Bolum?.BolumAd ?? "---",
                Durum = user.Aktiflik ? "Aktif" : "Pasif"
            }).ToList();

            UsersDataGrid.ItemsSource = usersForGrid;
        }

        // "Koordinatör Ekle" butonuna tıklandığında çalışacak olan metot.
        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Formdan verileri al.
            string adSoyad = AdSoyadTextBox.Text;
            string eposta = EpostaTextBox.Text;
            string sifre = SifrePasswordBox.Password;
            var secilenBolum = BolumComboBox.SelectedItem as Bolumler;

            // 2. Veri doğrulama (Validation).
            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(eposta) || string.IsNullOrWhiteSpace(sifre) || adSoyad == "Ad Soyad")
            {
                MessageBox.Show("Tüm alanlar doldurulmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (secilenBolum == null)
            {
                MessageBox.Show("Lütfen bir bölüm seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. UserService'i kullanarak kullanıcıyı ekle. Koordinatör'ün RolId'si 2'dir.
            bool success = _userService.AddUser(adSoyad, eposta, sifre, 2, secilenBolum.BolumId);

            // 4. Sonucu kontrol et ve kullanıcıya bildir.
            if (success)
            {
                MessageBox.Show("Koordinatör başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                // Formu temizle.
                AdSoyadTextBox.Text = "Ad Soyad";
                EpostaTextBox.Text = "E-posta";
                SifrePasswordBox.Clear();
                BolumComboBox.SelectedIndex = -1; // Seçimi kaldır.

                // Kullanıcı listesini yenile.
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Bu e-posta adresi zaten kullanılıyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

// Bu UserViewModel sınıfı, DataGrid'in verileri düzgün göstermesi için kullanılır.

// Gerçek veritabanı modelimizden ayrıdır.

public class UserViewModel

{
    public int Id { get; set; }
    public string AdSoyad { get; set; }
    public string Rol { get; set; }
    public string Eposta { get; set; }
    public string Bolum { get; set; }
    public string Durum { get; set; }

}

