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
            LoadBolumler();
            LoadDerslikler();
        }

        private void LoadBolumler()
        {
            var bolumler = _context.Bolumler.ToList();
            BolumComboBox.ItemsSource = bolumler;
            BolumComboBox.DisplayMemberPath = "BolumAd";
            BolumComboBox.SelectedValuePath = "BolumId";
            if (bolumler.Any())
                BolumComboBox.SelectedIndex = 0;
        }

        private void LoadDerslikler()
        {
            DersliklerDataGrid.ItemsSource = _context.Derslikler
                .Select(d => new
                {
                    d.DerslikId,
                    BolumAd = d.Bolum.BolumAd,
                    d.DerslikKod,
                    d.DerslikAd,
                    d.Kapasite,
                    d.EnineSira,
                    d.BoyunaSira,
                    d.SiraYapisi
                })
                .ToList();
        }

        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            if (BolumComboBox.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bölüm seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int bolumId = (int)BolumComboBox.SelectedValue;
            string kod = DerslikKoduTextBox.Text.Trim();
            string ad = DerslikAdTextBox.Text.Trim();
            string kapasiteStr = KapasiteTextBox.Text.Trim();
            string enineStr = EnineSiraTextBox.Text.Trim();
            string boyunaStr = BoyunaSiraTextBox.Text.Trim();
            string siraYapisiStr = (SiraYapisiComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();


            if (string.IsNullOrEmpty(kod) || string.IsNullOrEmpty(ad)
                || string.IsNullOrEmpty(kapasiteStr) || string.IsNullOrEmpty(enineStr)
                || string.IsNullOrEmpty(boyunaStr) || string.IsNullOrEmpty(siraYapisiStr))
            {
                MessageBox.Show("Tüm alanları doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(kapasiteStr, out int kapasite) || kapasite <= 0 ||
                !int.TryParse(enineStr, out int enine) || enine <= 0 ||
                !int.TryParse(boyunaStr, out int boyuna) || boyuna <= 0 ||
                !byte.TryParse(siraYapisiStr, out byte siraYapisi) || (siraYapisi != 2 && siraYapisi != 3))
            {
                MessageBox.Show("Sayı alanlarını doğru girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Derslikler.Any(d => d.DerslikKod == kod))
            {
                MessageBox.Show("Bu derslik kodu zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Derslikler.Any(d => d.DerslikAd == ad))
            {
                MessageBox.Show("Bu derslik adı zaten mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var yeniDerslik = new Derslikler
            {
                BolumId = bolumId,
                DerslikKod = kod,
                DerslikAd = ad,
                Kapasite = kapasite,
                EnineSira = enine,
                BoyunaSira = boyuna,
                SiraYapisi = siraYapisi
            };

            _context.Derslikler.Add(yeniDerslik);
            _context.SaveChanges();
            LoadDerslikler();
            ClearForm();

            MessageBox.Show("Derslik başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SilButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var derslikToDelete = _context.Derslikler
                .FirstOrDefault(d => d.DerslikId == (int)button.Tag);
            if (derslikToDelete == null) return;

            var result = MessageBox.Show($"'{derslikToDelete.DerslikAd}' dersliğini silmek istiyor musunuz?",
                                         "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            _context.Derslikler.Remove(derslikToDelete);
            _context.SaveChanges();
            LoadDerslikler();

            MessageBox.Show("Derslik başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearForm()
        {
            DerslikKoduTextBox.Clear();
            DerslikAdTextBox.Clear();
            KapasiteTextBox.Clear();
            EnineSiraTextBox.Clear();
            BoyunaSiraTextBox.Clear();
            SiraYapisiComboBox.SelectedIndex = -1;
            if (BolumComboBox.Items.Count > 0)
                BolumComboBox.SelectedIndex = 0;
        }
    }
}
