using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class BolumYonetimi : UserControl
    {
        private readonly AppDbContext _context;

        public BolumYonetimi()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadBolumler();
        }

        private void LoadBolumler()
        {
            BolumlerDataGrid.ItemsSource = _context.Bolumler.ToList();
        }

        private void BolumEkleButton_Click(object sender, RoutedEventArgs e)
        {
            string bolumAd = BolumAdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(bolumAd))
            {
                MessageBox.Show("Bölüm adı boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Bolumler.Any(b => b.BolumAd == bolumAd))
            {
                MessageBox.Show("Bu bölüm zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var yeniBolum = new Bolumler { BolumAd = bolumAd };
            _context.Bolumler.Add(yeniBolum);
            _context.SaveChanges();

            BolumAdTextBox.Clear();
            LoadBolumler();

            MessageBox.Show("Bölüm başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SilBolumButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bolumToDelete = button?.DataContext as Bolumler;
            if (bolumToDelete == null) return;

            var result = MessageBox.Show(
                $"'{bolumToDelete.BolumAd}' adlı bölümü silmek istiyor musunuz?",
                "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;

            _context.Bolumler.Remove(bolumToDelete);
            _context.SaveChanges();
            LoadBolumler();

            MessageBox.Show("Bölüm başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        
        private void BolumAdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BolumAdTextBox.Text))
                BolumAdPlaceholder.Visibility = Visibility.Visible;
            else
                BolumAdPlaceholder.Visibility = Visibility.Hidden;
        }
    }
}