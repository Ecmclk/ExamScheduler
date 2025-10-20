using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using ExamScheduler.Data.Parsers;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class DersYonetimi : UserControl
    {
        private readonly AppDbContext _context;
        private readonly DersService _dersService;
        private readonly DersListesiExcelParser _excelParser;

        public DersYonetimi()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _dersService = new DersService(_context);
            _excelParser = new DersListesiExcelParser();

            LoadBolumlerFiltre();
            _ = LoadDerslerAsync();
        }

        private void LoadBolumlerFiltre()
        {
            var bolumler = _context.Bolumler.ToList();
            bolumler.Insert(0, new Bolumler { BolumId = 0, BolumAd = "Tüm Bölümler" });

            BolumFiltreComboBox.ItemsSource = bolumler;
            BolumFiltreComboBox.DisplayMemberPath = "BolumAd";
            BolumFiltreComboBox.SelectedValuePath = "BolumId";
            BolumFiltreComboBox.SelectedIndex = 0;
        }

        private async Task LoadDerslerAsync()
        {
            try
            {
                // Her çağrıda yeni DbContext ve DersService oluştur
                using var context = new AppDbContext();
                var service = new DersService(context);

                var allDersler = await service.GetAllDerslerAsync();

                if (BolumFiltreComboBox.SelectedItem is Bolumler selectedBolum && selectedBolum.BolumId != 0)
                {
                    allDersler = allDersler.Where(d => d.BolumId == selectedBolum.BolumId).ToList();
                }

                DerslerDataGrid.ItemsSource = allDersler;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ders listesi yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void BolumFiltreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DerslerDataGrid != null)
                await LoadDerslerAsync();
        }

        private async void FiltreyiTemizleButton_Click(object sender, RoutedEventArgs e)
        {
            BolumFiltreComboBox.SelectedIndex = 0;
            await LoadDerslerAsync();
        }

        private async void SilDersButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not Dersler ders) 
                return;

            var result = MessageBox.Show($"'{ders.DersAdi}' adlı dersi silmek istiyor musunuz?",
                                         "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            _context.Dersler.Remove(ders);
            await _context.SaveChangesAsync();

            await LoadDerslerAsync();
            MessageBox.Show("Ders başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExcelYukleButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = null;

            // OpenFileDialog'u UI thread üzerinden aç
            await Dispatcher.InvokeAsync(() =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Dosyaları (.xlsx)|*.xlsx",
                    Title = "Ders Listesi Yükle"
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
                // Excel parser ile dosyayı oku
                var parseResult = await _excelParser.ParseAsync(filePath);

                if (parseResult.Errors.Any())
                {
                    var errors = string.Join("\n", parseResult.Errors.Select(er => er.Message));
                    MessageBox.Show($"Excel yüklenirken hatalar oluştu:\n{errors}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Dersleri servis aracılığıyla kaydet
                var saveResult = await _dersService.SaveDerslerAsync(parseResult);

                if (!saveResult.Success)
                {
                    var errors = string.Join("\n", saveResult.Errors);
                    MessageBox.Show($"Dersler kaydedilirken hatalar oluştu:\n{errors}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Excel dosyasından dersler başarıyla yüklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    ((MainPanelWindow)Application.Current.MainWindow)?.CheckIfExamMenuShouldBeVisible();

                }

                // DataGrid'i güncelle
                await LoadDerslerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel dosyası okunurken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DerslerDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (DerslerDataGrid.SelectedItem is not Dersler selectedDers)
                return;

            try
            {
                // Her çağrıda yeni context + servis
                using var context = new AppDbContext();
                var service = new DersService(context);

                var ogrenciler = await service.GetOgrencilerByDersIdAsync(selectedDers.DersId);

                var displayList = ogrenciler.Select(o => new
                {
                    OgrenciNo = o.OgrenciNo,
                    AdSoyad = o.AdSoyad
                }).ToList();

                OgrencilerDataGrid.ItemsSource = displayList;

            
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Öğrenci listesi yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
    
    
}
