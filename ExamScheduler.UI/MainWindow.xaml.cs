using ExamScheduler.Data;
using System.Windows;
using ExamScheduler.Data.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ExamScheduler.Services;

namespace ExamScheduler.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
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


        // YENİ EKLENEN METOT
       private async void BtnImportStudents_Click(object sender, RoutedEventArgs e)
{
    var filePath = SelectExcelFile();
    if (filePath == null) return;

    try
    {
        using (var context = new AppDbContext())
        {
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
