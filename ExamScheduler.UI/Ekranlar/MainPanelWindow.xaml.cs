using System.Windows;
using System.Windows.Controls;
using ExamScheduler.Data;
using ExamScheduler.Services;
using ExamScheduler.UI.Ekranlar;

namespace ExamScheduler.UI
{
    public partial class MainPanelWindow : Window
    {
        private readonly DersService _dersService;
        public MainPanelWindow()
        {
            InitializeComponent();
           
            var dbContext = new AppDbContext();
            _dersService = new DersService(dbContext);
            
            // Uygulama açıldığında varsayılan olarak Kontrol Paneli gelsin
            LoadUserControl(new KontrolPaneli());

            // 🔽 Eğer veriler önceden varsa sınav butonunu hemen kontrol et
            CheckIfExamMenuShouldBeVisible();
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
            MainContentControl.Content = new SinavProgramiOlustur(_dersService);
        }
    }
}