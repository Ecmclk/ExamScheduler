using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        /// <summary>
        /// ComboBox'ı (açılır listeyi) veritabanındaki bölümlerle doldurur.
        /// </summary>
        private void LoadBolumler()
        {
            var bolumler = _context.Bolumler.ToList();
            BolumComboBox.ItemsSource = bolumler;
            BolumComboBox.DisplayMemberPath = "BolumAd";
            BolumComboBox.SelectedValuePath = "BolumID";
        }

        /// <summary>
        /// Veritabanından kullanıcıları yükler ve DataGrid'e bağlar.
        /// </summary>
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

        /// <summary>
        /// Yeni koordinatör ekleme işlemi.
        /// </summary>
        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            string adSoyad = AdSoyadTextBox.Text.Trim();
            string eposta = EpostaTextBox.Text.Trim();
            string sifre = SifrePasswordBox.Password.Trim();
            var secilenBolum = BolumComboBox.SelectedItem as Bolumler;

            if (string.IsNullOrWhiteSpace(adSoyad) ||
                string.IsNullOrWhiteSpace(eposta) ||
                string.IsNullOrWhiteSpace(sifre))
            {
                MessageBox.Show("Tüm alanlar doldurulmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (secilenBolum == null)
            {
                MessageBox.Show("Lütfen bir bölüm seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Koordinatör'ün RolID'si 2 olarak kabul edilir
            bool success = _userService.AddUser(adSoyad, eposta, sifre, 2, secilenBolum.BolumId);

            if (success)
            {
                MessageBox.Show("Koordinatör başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                AdSoyadTextBox.Text = "Ad Soyad";
                EpostaTextBox.Text = "E-posta";
                SifrePasswordBox.Clear();
                BolumComboBox.SelectedIndex = -1;
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Bu e-posta adresi zaten sistemde kayıtlı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// DataGrid için sade görünümlü kullanıcı modeli.
    /// </summary>
    public class UserViewModel
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Rol { get; set; }
        public string Eposta { get; set; }
        public string Bolum { get; set; }
        public string Durum { get; set; }
    }
}