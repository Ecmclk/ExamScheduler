using ExamScheduler.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class KontrolPaneli : UserControl
    {
        private readonly AppDbContext _context;
        private readonly int _kullaniciRol;
        private readonly int? _kullaniciBolumId;
        public KontrolPaneli(int kullaniciRol, int? kullaniciBolumId = null,AppDbContext context = null)
        {
            InitializeComponent();
            _context = context ?? new AppDbContext();
            _kullaniciRol = kullaniciRol;
            _kullaniciBolumId = kullaniciBolumId;;
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
            (Window.GetWindow(this) as MainPanelWindow)?
                .LoadUserControl(new BolumYonetimi(_kullaniciRol));
        }

        private void DersButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?
                .LoadUserControl(new DersYonetimi(_kullaniciRol, _kullaniciBolumId));
        }

        private void OgrenciButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?
                .LoadUserControl(new OgrenciYonetimi(_kullaniciRol, _kullaniciBolumId));
        }

        private void KullaniciButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainPanelWindow)?
                .LoadUserControl(new KullaniciYonetimi(_kullaniciRol));
        }
        

        private void DerslikButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainPanelWindow;
            if (mainWindow != null)
            {
                mainWindow.LoadUserControl(
                    new DerslikYonetimi(_kullaniciRol, _kullaniciBolumId ?? 1, mainWindow.Context)
                );
            }

            
        }
    }
}