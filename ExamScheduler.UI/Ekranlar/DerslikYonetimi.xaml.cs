using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class DerslikYonetimi : UserControl
    {
        private readonly AppDbContext _context;

        public DerslikYonetimi()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadDerslikler();
        }

        /// <summary>
        /// Derslikleri veritabanından çeker ve DataGrid'e yükler.
        /// </summary>
        private void LoadDerslikler()
        {
            DersliklerDataGrid.ItemsSource = _context.Derslikler.ToList();
        }

        /// <summary>
        /// Yeni derslik ekleme işlemi.
        /// </summary>
        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            string derslikAd = DerslikAdTextBox.Text.Trim();
            string kapasiteStr = KapasiteTextBox.Text.Trim();

            if (string.IsNullOrEmpty(derslikAd) || string.IsNullOrEmpty(kapasiteStr))
            {
                MessageBox.Show("Derslik adı ve kapasite alanları boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(kapasiteStr, out int kapasite) || kapasite <= 0)
            {
                MessageBox.Show("Kapasite geçerli bir sayı olmalıdır.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Derslikler.Any(d => d.DerslikAd == derslikAd))
            {
                MessageBox.Show("Bu derslik zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var yeniDerslik = new Derslikler
            {
                DerslikAd = derslikAd,
                Kapasite = kapasite
            };

            _context.Derslikler.Add(yeniDerslik);
            _context.SaveChanges();
            LoadDerslikler();

            DerslikAdTextBox.Clear();
            KapasiteTextBox.Clear();

            MessageBox.Show("Derslik başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Derslik silme işlemi.
        /// </summary>
        private void SilButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var derslikToDelete = button?.DataContext as Derslikler;
            if (derslikToDelete == null) return;

            var result = MessageBox.Show($"'{derslikToDelete.DerslikAd}' adlı dersliği silmek istiyor musunuz?",
                                         "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            _context.Derslikler.Remove(derslikToDelete);
            _context.SaveChanges();
            LoadDerslikler();

            MessageBox.Show("Derslik başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void DerslikAdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DerslikAdPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(DerslikAdTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void KapasiteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            KapasitePlaceholder.Visibility =
                string.IsNullOrWhiteSpace(KapasiteTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}