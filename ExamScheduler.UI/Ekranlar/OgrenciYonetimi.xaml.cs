using System.Windows;
using System.Windows.Controls;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using Microsoft.Win32;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class OgrenciYonetimi : UserControl
    {
        private readonly AppDbContext _context;
        private readonly OgrenciService _ogrenciService;
        private readonly int _kullaniciRol;
        private readonly int? _kullaniciBolumId;

        public OgrenciYonetimi(int kullaniciRol, int? kullaniciBolumId = null)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _ogrenciService = new OgrenciService(_context);
            _kullaniciRol = kullaniciRol;
            _kullaniciBolumId = kullaniciBolumId;
            LoadBolumlerFiltre();
            _ = LoadOgrencilerAsync(); // async çağrı
        }

        private void LoadBolumlerFiltre()
        {
            var bolumler = _context.Bolumler.ToList();

            if (_kullaniciRol == 1) // Admin ise
            {
                bolumler.Insert(0, new Bolumler { BolumId = 0, BolumAd = "Tüm Bölümler" });
                BolumFiltreComboBox.ItemsSource = bolumler;
                BolumFiltreComboBox.DisplayMemberPath = "BolumAd";
                BolumFiltreComboBox.SelectedValuePath = "BolumId";
                BolumFiltreComboBox.SelectedIndex = 0;
                BolumFiltreComboBox.IsEnabled = true;
            }
            else // Bölüm koordinatörü ise
            {
                var kendiBolumu = bolumler.FirstOrDefault(b => b.BolumId == _kullaniciBolumId);
                if (kendiBolumu != null)
                {
                    BolumFiltreComboBox.ItemsSource = new List<Bolumler> { kendiBolumu };
                    BolumFiltreComboBox.DisplayMemberPath = "BolumAd";
                    BolumFiltreComboBox.SelectedValuePath = "BolumId";
                    BolumFiltreComboBox.SelectedIndex = 0;
                    BolumFiltreComboBox.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Kullanıcının kayıtlı olduğu bölüm bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadOgrencilerAsync()
        {
            try
            {
                // Her çağrıda yeni DbContext ve servis oluştur
                using var context = new AppDbContext();
                var service = new OgrenciService(context);

                var ogrenciler = await service.GetAllOgrencilerAsync();

                if (BolumFiltreComboBox.SelectedItem is Bolumler selectedBolum && selectedBolum.BolumId != 0)
                {
                    ogrenciler = ogrenciler.Where(o => o.BolumId == selectedBolum.BolumId).ToList();
                }

                OgrencilerDataGrid.ItemsSource = ogrenciler;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Öğrenci listesi yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void BolumFiltreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OgrencilerDataGrid != null)
                await LoadOgrencilerAsync();
        }

      
        
        private async void FiltreyiTemizleButton_Click(object sender, RoutedEventArgs e)
        {
            BolumFiltreComboBox.SelectedIndex = 0;
            await LoadOgrencilerAsync();
        }

        
        private async void SilOgrenciButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ogrenciToDelete = button?.DataContext as Ogrenciler;
            if (ogrenciToDelete == null) return;

            var result = MessageBox.Show($"'{ogrenciToDelete.AdSoyad}' adlı öğrenciyi silmek istiyor musunuz?",
                                         "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            _context.Ogrenciler.Remove(ogrenciToDelete);
            await _context.SaveChangesAsync();
            await LoadOgrencilerAsync();
            MessageBox.Show("Öğrenci başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

       private async void ExcelYukleButton_Click(object sender, RoutedEventArgs e)
{
    var bolumId = ((Bolumler)BolumFiltreComboBox.SelectedItem).BolumId;
    string filePath = null;

    // OpenFileDialog'u UI thread üzerinden aç
    await Dispatcher.InvokeAsync(() =>
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Dosyaları (.xlsx)|*.xlsx",
            Title = "Öğrenci Listesi Yükle"
        };

        // Owner window belirtmek
        bool? result = openFileDialog.ShowDialog(Application.Current.MainWindow);
        if (result == true)
            filePath = openFileDialog.FileName;
    });

    if (string.IsNullOrEmpty(filePath))
        return;

    try
    {
        var parser = new OgrenciListesiExcelParser();
        var parseResult = await parser.ParseAsync(filePath,bolumId);

        if (parseResult.Errors.Any())
        {
            MessageBox.Show(
                string.Join("\n", parseResult.Errors.Select(err => err.Message)),
                "Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        if (parseResult.Data.Any())
        {
            var saveResult = await _ogrenciService.SaveOgrenciDerslerAsync(parseResult);

            if (saveResult.Success)
            {
                await LoadOgrencilerAsync();
                MessageBox.Show(
                    "Excel dosyasından öğrenciler başarıyla yüklendi.",
                    "Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                
                ((MainPanelWindow)Application.Current.MainWindow)?.CheckIfExamMenuShouldBeVisible();

            }
            else
            {
                MessageBox.Show(
                    string.Join("\n", saveResult.Errors),
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Excel dosyası okunurken hata oluştu: {ex.Message}",
            "Hata",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}


        
        private async void OgrenciAraButton_Click(object sender, RoutedEventArgs e)
        {
            string ogrNo = OgrenciNoAramaTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ogrNo))
            {
                MessageBox.Show("Lütfen bir öğrenci numarası girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ogrenci = await _ogrenciService.GetOgrenciWithDerslerAsync(ogrNo);
            if (ogrenci == null)
            {
                OgrenciBilgiTextBlock.Text = "Öğrenci bulunamadı.";
                return;
            }

            // Bilgileri göster
            OgrenciBilgiTextBlock.Text = $"Öğrenci: {ogrenci.AdSoyad}\nAldığı Dersler:\n" +
                                         string.Join("\n", ogrenci.OgrenciDersler.Select(od => $"- {od.Ders.DersAdi} (Kodu: {od.Ders.DersKodu})"));
        }

    }

  
}
