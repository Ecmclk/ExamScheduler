using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class SinavProgramiOlustur : UserControl
    {
        private readonly DersService _dersService;
        private readonly OgrenciService _ogrenciService;
        private readonly AppDbContext _context;

        private List<SinavKaydi> _olusturulanProgram;

        public SinavProgramiOlustur(AppDbContext context)
        {
            InitializeComponent();

            _context = context ?? throw new ArgumentNullException(nameof(context));

            _dersService = new DersService(_context);
            _ogrenciService = new OgrenciService(_context);

            ProgramiOlusturButton.IsEnabled = false;

            Loaded += SinavProgramiOlustur_Loaded;
        }

        private async void SinavProgramiOlustur_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgramiOlusturButton.IsEnabled = false;
                await LoadBolumlerAsync();
                await LoadDerslerAsync();
                ProgramiOlusturButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Başlatılırken hata oluştu: {ex.Message}");
            }
        }

        private async Task<List<Bolumler>> LoadBolumlerAsync()
        {
            // Yeni context kullanıyoruz
            using var context = new AppDbContext();

            var bolumler = await context.Bolumler.ToListAsync();
            BolumComboBox.ItemsSource = bolumler;
            BolumComboBox.DisplayMemberPath = "BolumAd";
            BolumComboBox.SelectedValuePath = "BolumId";

            if (bolumler.Any())
            {
                var defaultBolum = bolumler.FirstOrDefault(b => b.BolumAd.Contains("Bilgisayar Mühendisligi"));
                BolumComboBox.SelectedValue = defaultBolum?.BolumId ?? bolumler.First().BolumId;
            }

            ProgramiOlusturButton.IsEnabled = true;
            return bolumler; // tekrar DB çağrısı yok
        }

        private async Task LoadDerslerAsync()
        {
            using var context = new AppDbContext();
            var dersService = new DersService(context);

            var dersler = await dersService.GetAllDerslerAsync();

            var dersViewModel = dersler.Select(d => new Dersler
            {
                DersId = d.DersId,
                DersAdi = d.DersAdi,
                Sinif = d.Sinif,
                OgretimGorevlisi = d.OgretimGorevlisi,
                Secili = true,
                SinavSuresi = d.SinavSuresi > 0 ? d.SinavSuresi : 75,
                BolumId = d.BolumId,
                Bolum = d.Bolum
            }).ToList();

            DersListBox.ItemsSource = dersViewModel;
        }

        private bool _isLoadingDersler = false;

        private async void BolumComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingDersler) return;

            _isLoadingDersler = true;
            try
            {
                if (BolumComboBox.SelectedItem is null) return;

                int seciliBolumId = (int)BolumComboBox.SelectedValue;

                using var context = new AppDbContext();
                var dersService = new DersService(context);
                var dersler = await dersService.GetAllDerslerAsync();

                var filtreliDersler = dersler
                    .Where(d => d.BolumId == seciliBolumId)
                    .Select(d => new Dersler
                    {
                        DersId = d.DersId,
                        DersAdi = d.DersAdi,
                        Sinif = d.Sinif,
                        OgretimGorevlisi = d.OgretimGorevlisi,
                        Secili = true,
                        SinavSuresi = d.SinavSuresi > 0 ? d.SinavSuresi : 75,
                        BolumId = d.BolumId,
                        Bolum = d.Bolum
                    })
                    .ToList();

                DersListBox.ItemsSource = filtreliDersler;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ders yüklenirken hata: {ex.Message}");
            }
            finally
            {
                _isLoadingDersler = false;
            }
        }
    



private async void ProgramiOlusturButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                var seciliDersler = DersListBox.ItemsSource
                    .Cast<Dersler>()
                    .Where(d => d.Secili)
                    .ToList();

                if (!seciliDersler.Any())
                {
                    MessageBox.Show("Lütfen en az bir ders seçin!");
                    return;
                }

                DateTime? start = StartDatePicker.SelectedDate;
                DateTime? end = EndDatePicker.SelectedDate;

                if (start == null || end == null || start > end)
                {
                    MessageBox.Show("Geçerli bir tarih aralığı seçiniz!");
                    return;
                }

                int beklemeSuresi = int.TryParse(BeklemeSuresiTextBox.Text, out var bs) ? bs : 15;
                bool haftaSonuYok = CumartesiPazarKisit.IsChecked == true;
                var haricTarihler = HaricTutulacakTarihlerCalendar.SelectedDates.ToHashSet();

                int seciliBolumId = (int)BolumComboBox.SelectedValue;
                var tumDerslikler = await _context.Derslikler
                    .Where(d => d.BolumId == seciliBolumId)
                    .ToListAsync();
                
                if (!tumDerslikler.Any())
                {
                    MessageBox.Show("Sistemde derslik bulunamadı!");
                    return;
                }

                var uygunTarihler = Enumerable.Range(0, (end.Value - start.Value).Days + 1)
                    .Select(i => start.Value.AddDays(i))
                    .Where(t => (!haftaSonuYok || (t.DayOfWeek != DayOfWeek.Saturday && t.DayOfWeek != DayOfWeek.Sunday)) &&
                                !haricTarihler.Contains(t))
                    .ToList();

                if (!uygunTarihler.Any())
                {
                    MessageBox.Show("Seçilen tarih aralığı sınav planlaması için uygun değil!");
                    return;
                }

                // Öğrenci-ders map sadece bir kez alınır
                var ogrenciDersMap = await _context.OgrenciDersler
                    .GroupBy(x => x.DersId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.OgrenciId).ToList());

                var sinavProgrami = new List<SinavKaydi>();
                var hataListesi = new List<string>();

                var sinifGruplari = seciliDersler.GroupBy(d => d.Sinif);
                foreach (var sinifGrup in sinifGruplari)
                {
                    var dersler = sinifGrup.ToList();
                    int dersSayisi = dersler.Count;
                    int gunSayisi = uygunTarihler.Count;

                    var dagilim = new List<int>(new int[gunSayisi]);
                    int kalan = dersSayisi;
                    int i = 0;
                    while (kalan > 0)
                    {
                        dagilim[i % gunSayisi]++;
                        kalan--;
                        i++;
                    }

                    int dersIndex = 0;
                    for (int g = 0; g < dagilim.Count; g++)
                    {
                        var tarih = uygunTarihler[g % gunSayisi];
                        int oGunSinavSayisi = dagilim[g];

                        for (int j = 0; j < oGunSinavSayisi && dersIndex < dersler.Count; j++)
                        {
                            var ders = dersler[dersIndex++];
                            var ogrenciIdListesi = ogrenciDersMap.TryGetValue(ders.DersId, out var liste)
                                ? liste
                                : new List<int>();

                            if (!ogrenciIdListesi.Any()) continue;

                            var gunIciSaatler = new List<TimeSpan>();
                            TimeSpan baslangic = new(9, 0, 0);
                            TimeSpan bitis = new(17, 0, 0);
                            TimeSpan adim = TimeSpan.FromMinutes(ders.SinavSuresi + beklemeSuresi);

                            for (var saat = baslangic; saat + TimeSpan.FromMinutes(ders.SinavSuresi) <= bitis; saat = saat.Add(adim))
                                gunIciSaatler.Add(saat);

                            TimeSpan? uygunSaat = null;
                            List<Derslikler> kullanilacakSalonlar = null;

                            foreach (var saat in gunIciSaatler)
                            {
                                bool ogrenciCakisiyorMu = sinavProgrami.Any(sp =>
                                    sp.Tarih == tarih &&
                                    sp.OgrenciIdListesi != null &&
                                    sp.OgrenciIdListesi.Any(id => ogrenciIdListesi.Contains(id)) &&
                                    SaatlerCakisiyorMu(sp.Saat, sp.SinavSuresi, saat, ders.SinavSuresi, beklemeSuresi)
                                );

                                if (ogrenciCakisiyorMu) continue;

                                var doluSalonlar = tumDerslikler
                                    .Where(salon => sinavProgrami.Any(sp =>
                                        sp.Tarih == tarih &&
                                        sp.Salon == salon.DerslikAd &&
                                        SaatlerCakisiyorMu(sp.Saat, sp.SinavSuresi, saat, ders.SinavSuresi, beklemeSuresi)
                                    ))
                                    .ToList();

                                var bosSalonlar = tumDerslikler
                                    .Where(salon => !doluSalonlar.Any(d => d.DerslikAd == salon.DerslikAd))
                                    .OrderByDescending(s => s.Kapasite)
                                    .ToList();

                                if (!bosSalonlar.Any()) continue;

                                int toplamKapasite = 0;
                                var kullanilacakSalonlarTemp = new List<Derslikler>();
                                foreach (var salon in bosSalonlar)
                                {
                                    kullanilacakSalonlarTemp.Add(salon);
                                    toplamKapasite += salon.Kapasite;
                                    if (toplamKapasite >= ogrenciIdListesi.Count) break;
                                }

                                if (toplamKapasite >= ogrenciIdListesi.Count)
                                {
                                    uygunSaat = saat;
                                    kullanilacakSalonlar = kullanilacakSalonlarTemp;
                                    break;
                                }
                            }

                            int toplamKapasiteSalonlar = kullanilacakSalonlar?.Sum(s => s.Kapasite) ?? 0;
                            if (uygunSaat == null || kullanilacakSalonlar == null || toplamKapasiteSalonlar < ogrenciIdListesi.Count)
                            {
                                hataListesi.Add($"Ders {ders.DersAdi} ({ders.Bolum?.BolumAd ?? "Bölüm"}): Uygun salon bulunamadı! ÖğrenciSayısı={ogrenciIdListesi.Count}, ToplamKapasite={toplamKapasiteSalonlar}");
                            }
                            else
                            {
                                int ogrenciIndex = 0;
                                foreach (var salon in kullanilacakSalonlar)
                                {
                                    int kapasite = salon.Kapasite;
                                    var ogrenciler = ogrenciIdListesi.Skip(ogrenciIndex).Take(kapasite).ToList();
                                    ogrenciIndex += ogrenciler.Count;

                                    sinavProgrami.Add(new SinavKaydi
                                    {
                                        DersId = ders.DersId,
                                        DersAdi = ders.DersAdi,
                                        Tarih = tarih,
                                        Saat = uygunSaat.Value.ToString(@"hh\:mm"),
                                        OgretimGorevlisi = ders.OgretimGorevlisi,
                                        BolumAd = ders.Bolum?.BolumAd ?? "BÖLÜM",
                                        Salon = salon.DerslikAd,
                                        SinavSuresi = ders.SinavSuresi,
                                        OgrenciIdListesi = ogrenciler
                                    });

                                    if (ogrenciIndex >= ogrenciIdListesi.Count) break;
                                }
                            }
                        }
                    }
                }

                if (hataListesi.Any())
                {
                    MessageBox.Show(string.Join("\n", hataListesi), "Hatalar", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                SinavProgramiDataGrid.ItemsSource = sinavProgrami;
                _olusturulanProgram = sinavProgrami;
                ExcelIndirButton.Visibility = Visibility.Visible;

                MessageBox.Show("Sınav programı başarıyla oluşturuldu!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaatlerCakisiyorMu(string mevcutSaatStr, int mevcutSure, TimeSpan yeniBaslangic, int yeniSure, int bekleme)
        {
            var mevcutBaslangic = TimeSpan.Parse(mevcutSaatStr);
            var mevcutBitis = mevcutBaslangic.Add(TimeSpan.FromMinutes(mevcutSure));
            var yeniBitis = yeniBaslangic.Add(TimeSpan.FromMinutes(yeniSure + bekleme));

            return !(yeniBitis <= mevcutBaslangic || yeniBaslangic >= mevcutBitis);
        }

       private void ExcelIndirButton_Click(object sender, RoutedEventArgs e)
{
    if (_olusturulanProgram == null || !_olusturulanProgram.Any())
    {
        MessageBox.Show("Önce sınav programı oluşturun!");
        return;
    }

    var saveDialog = new SaveFileDialog
    {
        Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
        Title = "Sınav Programını Kaydet",
        FileName = "SinavProgrami.xlsx"
    };

    if (saveDialog.ShowDialog() != true) return;

    string filePath = saveDialog.FileName;

    try
    {
        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("SINAV PROGRAMI");

            string sinavTuru = SinavTuruComboBox.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "";
            string bolumAdi = BolumComboBox.Text ?? "BÖLÜM";
            string baslik = $"{bolumAdi.ToUpper()} BÖLÜMÜ {sinavTuru?.ToUpper()} SINAV PROGRAMI";

            // Başlık
            var headerRange = ws.Range("A1:E1");
            headerRange.Merge();
            headerRange.Value = baslik;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 14;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.Orange;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Başlık satırı
            string[] basliklar = { "Tarih", "Sınav Saati", "Ders Adı", "Öğretim Elemanı", "Derslik" };
            for (int i = 0; i < basliklar.Length; i++)
            {
                var cell = ws.Cell(2, i + 1);
                cell.Value = basliklar[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.Orange;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Veri satırları
            var satirListesi = _olusturulanProgram
                .GroupBy(k => new { k.Tarih, k.Saat, k.DersAdi, k.OgretimGorevlisi })
                .OrderBy(g => g.Key.Tarih).ThenBy(g => g.Key.Saat)
                .Select(grp => new
                {
                    grp.Key.Tarih,
                    grp.Key.Saat,
                    grp.Key.DersAdi,
                    grp.Key.OgretimGorevlisi,
                    Derslik = string.Join(", ", grp.Select(g => g.Salon))
                })
                .ToList();

            int startRow = 3;
            while (startRow <= satirListesi.Count + 2)
            {
                DateTime currentTarih = satirListesi[startRow - 3].Tarih;
                int endRow = startRow;

                while (endRow - 3 < satirListesi.Count && satirListesi[endRow - 3].Tarih == currentTarih)
                    endRow++;

                endRow--;

                // Tarih hücresi dikey olarak birleştir
                ws.Range(startRow, 1, endRow, 1).Merge();
                ws.Cell(startRow, 1).Value = currentTarih.ToString("dd.MM.yyyy");
                ws.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(startRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(startRow, 1).Style.Alignment.TextRotation = 90;

                // Diğer hücreler
                for (int r = startRow; r <= endRow; r++)
                {
                    var satir = satirListesi[r - 3];
                    ws.Cell(r, 2).Value = satir.Saat;
                    ws.Cell(r, 3).Value = satir.DersAdi;
                    ws.Cell(r, 4).Value = satir.OgretimGorevlisi;
                    ws.Cell(r, 5).Value = satir.Derslik;

                    for (int c = 2; c <= 5; c++)
                    {
                        var cell = ws.Cell(r, c);
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.TextRotation = 0;
                    }
                }

                startRow = endRow + 1;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        MessageBox.Show("Excel dosyası başarıyla oluşturuldu!");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Excel kaydedilirken bir hata oluştu: {ex.Message}");
    }
}

    }

    public class SinavKaydi
    {
        public int DersId { get; set; }
        public string DersAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string OgretimGorevlisi { get; set; }
        public string Saat { get; set; }
        public string Salon { get; set; }
        public string BolumAd { get; set; } = "";
        public int SinavSuresi;
        public List<int>? OgrenciIdListesi { get; set; }
    }
}
