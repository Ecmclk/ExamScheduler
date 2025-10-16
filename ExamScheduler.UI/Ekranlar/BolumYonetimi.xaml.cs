using ExamScheduler.Data;
using ExamScheduler.Data;
using ExamScheduler.Models;
using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        /// <summary>
        /// Veritabanındaki tüm bölümleri DataGrid'e yükler.
        /// </summary>
        private void LoadBolumler()
        {
            BolumlerDataGrid.ItemsSource = _context.Bolumler.ToList();
        }

        /// <summary>
        /// "Bölüm Ekle" butonuna tıklandığında çalışır.
        /// </summary>
        private void EkleBolumButton_Click(object sender, RoutedEventArgs e)
        {
            string bolumAd = BolumAdTextBox.Text;
            string bolumKod = BolumKodTextBox.Text;

            // Alanların boş olup olmadığını kontrol et.
            if (string.IsNullOrWhiteSpace(bolumAd) || string.IsNullOrWhiteSpace(bolumKod))
            {
                MessageBox.Show("Bölüm adı ve kodu boş bırakılamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Yeni bir Bolumler nesnesi oluştur.
            var yeniBolum = new Bolumler
            {
                BolumAd = bolumAd,
                BolumKod = bolumKod
            };

            // Veritabanına ekle ve kaydet.
            _context.Bolumler.Add(yeniBolum);
            _context.SaveChanges();

            // Formu temizle ve listeyi yenile.
            BolumAdTextBox.Clear();
            BolumKodTextBox.Clear();
            LoadBolumler();

            MessageBox.Show("Bölüm başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// "Sil" butonuna tıklandığında çalışır.
        /// </summary>
        private void SilBolumButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bolumToDelete = button?.DataContext as Bolumler;
            if (bolumToDelete == null) return;

            // Kullanıcıya onay sorusu sor.
            var result = MessageBox.Show($"'{bolumToDelete.BolumAd}' adlı bölümü silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            // Veritabanından sil ve kaydet.
            _context.Bolumler.Remove(bolumToDelete);
            _context.SaveChanges();

            // Listeyi yenile.
            LoadBolumler();
            MessageBox.Show("Bölüm başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}