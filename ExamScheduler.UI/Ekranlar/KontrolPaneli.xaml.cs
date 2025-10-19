using ExamScheduler.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class KontrolPaneli : UserControl
    {
        private readonly AppDbContext _context;

        public KontrolPaneli()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            BolumSayisiText.Text = _context.Bolumler.Count().ToString();
            DersSayisiText.Text = _context.Dersler.Count().ToString();
            OgrenciSayisiText.Text = _context.Ogrenciler.Count().ToString();
            KullaniciSayisiText.Text = _context.Kullanicilar.Count().ToString();
            DerslikSayisiText.Text = _context.Derslikler.Count().ToString();
        }

        // Alt ekranlara yönlendirme (MainPanelWindow içinde tanımlı Content alanına geçiş)
        private void BolumButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?.LoadUserControl(new BolumYonetimi());
        }

        private void DersButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?.LoadUserControl(new DersYonetimi());
        }

        private void OgrenciButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?.LoadUserControl(new OgrenciYonetimi());
        }

        private void KullaniciButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?.LoadUserControl(new KullaniciYonetimi());
        }

        private void DerslikButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?.LoadUserControl(new DerslikYonetimi());
        }
    }
}