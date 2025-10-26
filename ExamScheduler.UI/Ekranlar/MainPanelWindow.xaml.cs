using System.Windows;
using System.Windows.Controls;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using ExamScheduler.UI.Ekranlar;

namespace ExamScheduler.UI
{
    public partial class MainPanelWindow : Window
    {
        private readonly DersService _dersService;
        private readonly OgrenciService _ogrenciService;
        private readonly AppDbContext _context;
  
        private List<SinavKaydi> _olusturulanProgram;
        public List<SinavKaydi> OlusturulanProgram => _olusturulanProgram;

        public SinavKaydi SonOluşturulanSinav { get; set; }
        public List<Ogrenciler> SonSinavOgrencileri { get; set; }
        public MainPanelWindow()
        {
            InitializeComponent();

            // ✅ Context'i burada başlat
            _context = new AppDbContext();

            // ✅ Servisleri context ile başlat
            _dersService = new DersService(_context);
            _ogrenciService = new OgrenciService(_context);

            // Varsayılan olarak kontrol paneli gelsin
            LoadUserControl(new KontrolPaneli());
            

            // Menü kontrolü
            _ = CheckIfExamMenuShouldBeVisible();
            
        }

        /// <summary>
        /// Verilen UserControl'ü içerik alanına yükler.
        /// </summary>
        public void LoadUserControl(UserControl control)
        {
            MainContentControl.Content = control;
        }

        private void KontrolPaneliButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new KontrolPaneli());
        }

        private void BolumButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new BolumYonetimi());
        }

        private void DersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new DersYonetimi());
        }

        private void OgrenciButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new OgrenciYonetimi());
        }

        private void KullaniciButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new KullaniciYonetimi());
        }

        private void DerslikButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new DerslikYonetimi());
        }

        private void CikisButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Oturumu kapatmak istediğinize emin misiniz?",
                    "Çıkış Onayı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                this.Close();
            }
        }
        
        public async Task  CheckIfExamMenuShouldBeVisible()
        {
            var dersler = await _dersService.GetAllDerslerAsync();
            bool hasDers = dersler.Any();

            // Örnek: öğrenci kontrolü için OgrenciService kullanabilirsin
            bool hasOgrenci = true; // veya servis ile kontrol et

            SinavProgramiButton.Visibility = (hasDers && hasOgrenci)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SinavProgrami_Click(object sender, RoutedEventArgs e)
        {
            var sinavKontrol = new SinavProgramiOlustur(_context);

            // Event ile _olusturulanProgram’ı al
            sinavKontrol.SinavProgramiOlusturuldu += (program) =>
            {
                _olusturulanProgram = program; // tüm dersler burada saklanır
            };

            LoadUserControl(sinavKontrol);
          
        }

        private void OturmaPlani_Click(object sender, RoutedEventArgs e)
        {
            if (_olusturulanProgram == null || !_olusturulanProgram.Any())
            {
                MessageBox.Show("Önce sınav programı oluşturmalısınız!");
                return;
            }

            var oturmaPlaniControl = new OturmaPlani(_context);
            oturmaPlaniControl.SetSinavProgrami(_olusturulanProgram); // tüm dersler gönderiliyor
            LoadUserControl(oturmaPlaniControl);
        }

    }
}