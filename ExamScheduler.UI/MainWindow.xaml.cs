using ExamScheduler.Data;
using System;
using System.Linq;
using System.Windows;

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

        private async void BtnImportCourses_Click(object sender, RoutedEventArgs e)
        {
            var filePath = SelectExcelFile();
            if (filePath == null) return;

            try
            {
                using var db = new AppDbContext();
                var parser = new DersListesiExcelParser();
                var result = await parser.ParseAsync(filePath);

                if (result.Errors.Any())
                {
                    MessageBox.Show(
                        string.Join("\n", result.Errors.Select(x => $"Satır {x.RowNumber}: {x.Message}")),
                        "Excel Okuma Hataları (Ders Listesi)",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }

                if (result.Data.Any())
                {
                    // Veritabanında zaten var olan dersleri tekrar eklememek için kontrol et
                    var existingCourseCodes = await db.Dersler.Select(d => d.DersKodu).ToListAsync();
                    var newCourses = result.Data.Where(d => !existingCourseCodes.Contains(d.DersKodu)).ToList();

                    if (newCourses.Any())
                    {
                        db.Dersler.AddRange(newCourses);
                        await db.SaveChangesAsync();

                        MessageBox.Show(
                            $"Toplam {newCourses.Count} yeni ders başarıyla kaydedildi!",
                            "Başarılı",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                    else
                    {
                        MessageBox.Show("Tüm dersler zaten veritabanında mevcut. Yeni ders eklenmedi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Kaydedilecek ders bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Bağlantının gerçekten kurulabildiğini test etmek için basit bir sorgu
                    var bolumSayisi = context.Bolumler.Count();

                    MessageBox.Show($"✅ Veritabanı bağlantısı başarılı!",
                                    "Bağlantı Testi",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }

                int changes = await db.SaveChangesAsync();
                MessageBox.Show($"{changes} değişiklik (yeni öğrenci ve ders kayıtları) başarıyla veritabanına kaydedildi.", "İşlem Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Hata: {ex.Message}",
                                "Bağlantı Hatası",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
