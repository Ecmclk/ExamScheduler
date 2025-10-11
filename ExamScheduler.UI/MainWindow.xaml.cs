using ExamScheduler.Data;
using ExamScheduler.Data.Parsers;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
                using var db = new AppDbContext();
                var parser = new OgrenciListesiExcelParser();
                var result = await parser.ParseAsync(filePath); // `OgrenciDersler` listesi döner

                if (result.Errors.Any())
                {
                    MessageBox.Show(
                        string.Join("\n", result.Errors.Select(x => $"Satır {x.RowNumber}: {x.Message}")),
                        "Excel Okuma Hataları (Öğrenci Listesi)",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }

                if (!result.Data.Any())
                {
                    MessageBox.Show("Kaydedilecek öğrenci veya ders ilişkisi bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 1. ADIM: Excel'den gelen benzersiz öğrenci ve ders kodlarını al
                var studentNumbersFromFile = result.Data.Select(od => od.Ogrenci.OgrenciNo).Distinct().ToList();
                var courseCodesFromFile = result.Data.Select(od => od.Ders.DersKodu).Distinct().ToList();

                // 2. ADIM: Veritabanından ilgili mevcut öğrencileri ve dersleri tek seferde çek
                var existingStudents = await db.Ogrenciler
                    .Where(o => studentNumbersFromFile.Contains(o.OgrenciNo))
                    .ToDictionaryAsync(o => o.OgrenciNo);

                var existingCourses = await db.Dersler
                    .Where(d => courseCodesFromFile.Contains(d.DersKodu))
                    .ToDictionaryAsync(d => d.DersKodu);

                // 3. ADIM: Eklenecek yeni öğrencileri ve ilişkileri belirle
                var studentsToAdd = new List<Ogrenciler>();
                var relationsToAdd = new List<OgrenciDersler>();
                var invalidCourseErrors = new List<string>();

                // Excel'den okunan benzersiz öğrencileri işle
                foreach (var parsedStudent in result.Data.Select(od => od.Ogrenci).DistinctBy(o => o.OgrenciNo))
                {
                    if (!existingStudents.ContainsKey(parsedStudent.OgrenciNo))
                    {
                        studentsToAdd.Add(parsedStudent);
                        existingStudents.Add(parsedStudent.OgrenciNo, parsedStudent); // Yeni öğrenciyi de "mevcut" listesine ekle
                    }
                }

                // Excel'den okunan her bir öğrenci-ders ilişkisini işle
                foreach (var parsedRelation in result.Data)
                {
                    // Dersin veritabanında kayıtlı olup olmadığını kontrol et
                    if (!existingCourses.TryGetValue(parsedRelation.Ders.DersKodu, out var courseEntity))
                    {
                        invalidCourseErrors.Add($"'{parsedRelation.Ders.DersKodu}' kodlu ders veritabanında bulunamadı. Öğrenci: {parsedRelation.Ogrenci.OgrenciNo}");
                        continue;
                    }

                    // Öğrenciyi al (yeni veya mevcut)
                    var studentEntity = existingStudents[parsedRelation.Ogrenci.OgrenciNo];

                    // İlişkiyi oluştur
                    relationsToAdd.Add(new OgrenciDersler { Ogrenci = studentEntity, Ders = courseEntity });
                }

                if (invalidCourseErrors.Any())
                {
                    MessageBox.Show(string.Join("\n", invalidCourseErrors.Distinct()), "Geçersiz Dersler", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 4. ADIM: Veritabanına kaydet
                // Önce mevcut tüm ilişkileri silerek temiz bir başlangıç yapabiliriz (opsiyonel)
                // db.OgrenciDersler.RemoveRange(db.OgrenciDersler);
                // db.Ogrenciler.RemoveRange(db.Ogrenciler);
                // await db.SaveChangesAsync();

                if (studentsToAdd.Any())
                {
                    db.Ogrenciler.AddRange(studentsToAdd);
                }

                if (relationsToAdd.Any())
                {
                    // Duplikasyonları engellemek için sadece veritabanında henüz olmayan ilişkileri ekle
                    var existingRelations = await db.OgrenciDersler
                        .Select(od => od.Ogrenci.OgrenciNo + "|" + od.Ders.DersKodu)
                        .ToHashSetAsync();

                    var newRelations = relationsToAdd
                        .Where(r => !existingRelations.Contains(r.Ogrenci.OgrenciNo + "|" + r.Ders.DersKodu))
                        .ToList();

                    if (newRelations.Any())
                    {
                        db.OgrenciDersler.AddRange(newRelations);
                    }
                }

                int changes = await db.SaveChangesAsync();
                MessageBox.Show($"{changes} değişiklik (yeni öğrenci ve ders kayıtları) başarıyla veritabanına kaydedildi.", "İşlem Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.InnerException?.Message ?? ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string? SelectExcelFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Excel Dosyası Seç",
                Filter = "Excel Dosyaları (*.xlsx)|*.xlsx|Tüm Dosyalar (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true) return null;

            return openFileDialog.FileName;
        }
    }

}
