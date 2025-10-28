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
        private readonly int _kullaniciRol;
        private readonly int? _kullaniciBolumId;
  
        private List<SinavKaydi> _olusturulanProgram;
        public List<SinavKaydi> OlusturulanProgram => _olusturulanProgram;
        public AppDbContext Context => _context;

        public SinavKaydi SonOluşturulanSinav { get; set; }
        public List<Ogrenciler> SonSinavOgrencileri { get; set; }
        public MainPanelWindow(int kullaniciRol, int? kullaniciBolumId = null)
        {
            InitializeComponent();
    
            _kullaniciRol = kullaniciRol; 
            _kullaniciBolumId = kullaniciBolumId;
    
            _context = new AppDbContext();
            _dersService = new DersService(_context);
            _ogrenciService = new OgrenciService(_context);

            LoadUserControl(new KontrolPaneli(_kullaniciRol, _kullaniciBolumId));

            Loaded += MainPanelWindow_Loaded; // Window yüklendikten sonra menüleri güncelle
        }

        private async void MainPanelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckIfExamMenuShouldBeVisible();
            UpdateMenuAvailability();
        }
        public void UpdateMenuAvailability()
        {
            if (_kullaniciRol == 1) // Admin her zaman açık
            {
                DersButton.IsEnabled = true;
                OgrenciButton.IsEnabled = true;
                SinavProgramiButton.IsEnabled = true;
            }
            else // Koordinatör
            {
                bool derslikVar = _context.Derslikler.Any(d => d.BolumId == _kullaniciBolumId);
                DersButton.IsEnabled = derslikVar;
                OgrenciButton.IsEnabled = derslikVar;
                SinavProgramiButton.IsEnabled = derslikVar;
            }
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
            LoadUserControl(new KontrolPaneli(_kullaniciRol, _kullaniciBolumId));
        }

        private void BolumButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new BolumYonetimi(_kullaniciRol));
        }

        private void DersButton_Click(object sender, RoutedEventArgs e)
        {
            var dersYonetimi = new DersYonetimi(_kullaniciRol, _kullaniciBolumId);
            LoadUserControl(dersYonetimi);
            
        }

        private void OgrenciButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new OgrenciYonetimi(_kullaniciRol, _kullaniciBolumId));
        }

        private void KullaniciButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new KullaniciYonetimi(_kullaniciRol));
        }

        private void DerslikButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserControl(new DerslikYonetimi(_kullaniciRol, _kullaniciBolumId ?? 1, _context));

            
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
            var sinavKontrol = new SinavProgramiOlustur(_context, _kullaniciRol, _kullaniciBolumId);


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