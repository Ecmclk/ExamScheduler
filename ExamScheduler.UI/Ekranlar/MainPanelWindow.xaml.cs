using System.Windows;
using System.Windows.Controls;
using ExamScheduler.UI.Ekranlar;

namespace ExamScheduler.UI
{
    public partial class MainPanelWindow : Window
    {
        public MainPanelWindow()
        {
            InitializeComponent();

            // Uygulama açıldığında varsayılan olarak Kontrol Paneli gelsin
            LoadUserControl(new KontrolPaneli());
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
    }
}