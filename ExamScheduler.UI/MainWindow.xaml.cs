using ExamScheduler.Data;
using System.Windows;
using System.Windows.Controls;
using ExamScheduler.Data.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ExamScheduler.Services;

namespace ExamScheduler.UI
{
    public partial class MainWindow : Window
    {

        private readonly OgrenciService _ogrenciService;
        private readonly DersService _dersService;
        public MainWindow()
        {
            _ogrenciService = new OgrenciService(new AppDbContext());
            _dersService = new DersService(new AppDbContext());
            InitializeComponent();
            TestEt();
        }

        private void TestEt()
        {
            try
            {
                using var db = new AppDbContext();
                db.Database.Migrate(); // Veritabanı yoksa oluştur

                MessageBox.Show("✅ Veritabanı bağlantısı başarılı!", "Bağlantı Testi",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Hata: {ex.Message}", "Bağlantı Hatası",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string? SelectExcelFile()
        {
            // Dosya seçim penceresi oluştur
            var openFileDialog = new OpenFileDialog
            {
                Title = "Excel Dosyası Seç",
                Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|Tüm Dosyalar (*.*)|*.*",
                Multiselect = false
            };

            // Kullanıcı dosya seçtiyse yolu döndür
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            // Hiç dosya seçmediyse null döndür
            MessageBox.Show("Dosya seçilmedi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }
        
        //Listeleme Menüsü Fonksiyonları
        private async void BtnShowCourses_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flatList = await _dersService.GetFlatDersListAsync();
                DataGridMain.ItemsSource = flatList;

                // Öğrenci DataGrid’ini temizle
                DataGridOgrenciler.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }


        private async void BtnShowStudents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flatList = await _ogrenciService.GetAllOgrencilerAsync();
                DataGridMain.ItemsSource = flatList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        //Search Butonu Fonksiyonları
        private async void BtnSearchStudent_Click(object sender, RoutedEventArgs e)
        {
            string ogrNo = TxtOgrenciNo.Text.Trim();
            if (string.IsNullOrEmpty(ogrNo)) return;

            var studentService = new OgrenciService(new AppDbContext());
            var ogrenci = await studentService.GetOgrenciWithDerslerAsync(ogrNo);

            if (ogrenci == null)
            {
                MessageBox.Show("Öğrenci bulunamadı.");
                return;
            }

            // Öğrenci adı-soyad
            LblOgrenciAdSoyad.Content = $"Öğrenci: {ogrenci.AdSoyad}";

            // Dersleri alt alta göstermek
            var dersList = ogrenci.OgrenciDersler
                .Select(od => $"{od.Ders.DersAdi} (Kodu: {od.Ders.DersKodu})")
                .ToList();

            LstOgrenciDersler.ItemsSource = dersList; // <-- ListBox kullanıyoruz
        }

        private async void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridMain.SelectedItem == null) return;

            var selected = DataGridMain.SelectedItem;

            // Anonymous type veya DataRowView kontrolü
            int dersId = 0;
            var prop = selected.GetType().GetProperty("DersId");
            if (prop != null)
            {
                dersId = (int)prop.GetValue(selected)!;
            }
            else if (selected is System.Data.DataRowView drv && drv.Row.Table.Columns.Contains("DersId"))
            {
                dersId = Convert.ToInt32(drv["DersId"]);
            }
            else
            {
                MessageBox.Show("Seçilen satırdan DersId alınamadı.");
                return;
            }

            await ShowOgrenciler(dersId);
        }


        private async Task ShowOgrenciler(int dersId)
        {
            var ogrenciler = await _dersService.GetOgrencilerByDersIdAsync(dersId);
            DataGridOgrenciler.ItemsSource = ogrenciler
                .Select(o => new { o.OgrenciNo, o.AdSoyad, o.Sinif })
                .ToList();
        }
        
        //Excel Dosyası Okuma Fonksiyonları
        private async void BtnImportCourses_Click(object sender, RoutedEventArgs e)
        {
            var filePath = SelectExcelFile();
            if (filePath == null) return;

            try
            {
                using var context = new AppDbContext();
                var parser = new DersListesiExcelParser();
                var parseResult = await parser.ParseAsync(filePath);

                if (parseResult.Errors.Any())
                {
                    MessageBox.Show(
                        string.Join("\n", parseResult.Errors.Select(x => $"Satır {x.RowNumber}: {x.Message}")),
                        "Excel Okuma Hataları (Ders Listesi)",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }

                if (parseResult.Data.Any())
                {
                    var dersService = new DersService(context);
                    var saveResult = await dersService.SaveDerslerAsync(parseResult);

                    if (saveResult.Errors.Any())
                    {
                        MessageBox.Show(
                            string.Join("\n", saveResult.Errors),
                            "Kayıt Hataları",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            $"✅ {parseResult.Data.Count} ders başarıyla kaydedildi.",
                            "Başarılı",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


 
       private async void BtnImportStudents_Click(object sender, RoutedEventArgs e)
        {
            var filePath = SelectExcelFile();
            if (filePath == null) return; 
         
             try {
                using (var context = new AppDbContext()) {
            // 1️⃣ Parser'ı çalıştır
            var parser = new OgrenciListesiExcelParser();
            var parseResult = await parser.ParseAsync(filePath);

            // 2️⃣ Hataları göster
            if (parseResult.Errors.Any())
            {
                MessageBox.Show(
                    string.Join("\n", parseResult.Errors.Select(x => $"Satır {x.RowNumber}: {x.Message}")),
                    "Excel Okuma Hataları",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            // 3️⃣ Veriyi DB'ye kaydet
            if (parseResult.Data.Any())
            {
                var ogrenciService = new OgrenciService(context);
                var saveResult = await ogrenciService.SaveOgrenciDerslerAsync(parseResult);

                if (saveResult.Errors.Any())
                {
                    MessageBox.Show(
                        string.Join("\n", saveResult.Errors),
                        "Kayıt Hataları",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                else
                {
                    MessageBox.Show(
                        $"✅ {parseResult.Data.Count} öğrenci ve ders kaydı başarıyla eklendi.",
                        "Başarılı",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            else
            {
                MessageBox.Show("Kaydedilecek öğrenci verisi bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"❌ Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

    }
}
