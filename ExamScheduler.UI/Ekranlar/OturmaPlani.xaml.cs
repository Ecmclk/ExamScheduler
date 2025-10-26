using DocumentFormat.OpenXml.Drawing;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Linq;

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class OturmaPlani : UserControl
    {
        private readonly AppDbContext _context;
        private List<Ogrenciler> _dersOgrencileri = new();
        private List<Derslikler> _secilenDerslikler = new();
        private Dersler? _seciliDers;

        public OturmaPlani()
        {
            InitializeComponent();
            _context = new AppDbContext();
            YukleDersler();
        }

        // ------------------- DERSLERİ GETİR -------------------
        private void YukleDersler()
        {
            var dersler = _context.Dersler
                                  .Include(d => d.Bolum)
                                  .OrderBy(d => d.DersAdi)
                                  .ToList();

            DersComboBox.ItemsSource = dersler;
            DersComboBox.DisplayMemberPath = "DersAdi";
            DersComboBox.SelectedValuePath = "DersId";

            if (dersler.Count > 0)
                DersComboBox.SelectedIndex = 0;
        }

        // ------------------- DERS SEÇİLDİĞİNDE -------------------
        private void DersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SalonTabs.Items.Clear();
            OturmaListesiGrid.ItemsSource = null;
            BilgiText.Text = "";

            if (DersComboBox.SelectedItem is not Dersler seciliDers)
                return;

            _seciliDers = seciliDers;

            // Bu dersi alan öğrencileri çek
            _dersOgrencileri = _context.OgrenciDersler
                                       .Where(od => od.DersId == seciliDers.DersId)
                                       .Include(od => od.Ogrenci)
                                       .Select(od => od.Ogrenci)
                                       .Distinct()
                                       .OrderBy(o => o.OgrenciNo)
                                       .ToList();

            // Bu dersin bölümüne ait derslikleri getir
            var derslikler = _context.Derslikler
                                     .Include(x => x.Bolum)
                                     .Where(x => x.BolumId == seciliDers.BolumId)
                                     .OrderByDescending(x => x.Kapasite)
                                     .ToList();

            DerslikListBox.ItemsSource = derslikler;
            DerslikListBox.SelectedItems.Clear();

            BilgiText.Text =
                $"Seçili Ders: {seciliDers.DersAdi} | Öğrenci Sayısı: {_dersOgrencileri.Count} | Uygun Derslik: {derslikler.Count}";
        }

        // ------------------- PLAN OLUŞTUR -------------------
        private void PlanOlusturButton_Click(object sender, RoutedEventArgs e)
        {
            if (_seciliDers == null)
            {
                MessageBox.Show("Lütfen bir ders seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _secilenDerslikler = DerslikListBox.SelectedItems.Cast<Derslikler>().ToList();

            if (_secilenDerslikler.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir derslik seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dersOgrencileri.Count == 0)
            {
                MessageBox.Show("Bu derse kayıtlı öğrenci bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                SalonTabs.Items.Clear();
                OturmaListesiGrid.ItemsSource = null;
                return;
            }

            int toplamKapasite = _secilenDerslikler.Sum(x => x.Kapasite);
            if (toplamKapasite < _dersOgrencileri.Count)
            {
                MessageBox.Show(
                    $"Seçilen dersliklerin toplam kapasitesi yetersiz!\nToplam Kapasite: {toplamKapasite}, Öğrenci Sayısı: {_dersOgrencileri.Count}",
                    "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Öğrencileri rastgele karıştır (daha adil dağıtım için)
            _dersOgrencileri = _dersOgrencileri.OrderBy(x => Guid.NewGuid()).ToList();

            // Öğrencileri dersliklere sırayla ata
            var atamalar = DagitOgrenciler(_dersOgrencileri, _secilenDerslikler);

            // Sekmelerde oturma planlarını çiz
            var gorunums = AtamalariGorunumeCevir(atamalar);
            CizSekmeler(gorunums);

            // Listeyi göster
            OturmaListesiGrid.ItemsSource = atamalar
                .OrderBy(a => a.DerslikAd)
                .ThenBy(a => a.SiraNo)
                .ThenBy(a => a.SutunNo)
                .Select(a => new
                {
                    a.OgrenciNo,
                    a.AdSoyad,
                    a.DerslikAd,
                    a.SiraNo,
                    a.SutunNo
                })
                .ToList();

            BilgiText.Text =
                $"Plan oluşturuldu • Ders: {_seciliDers.DersAdi} • Öğrenci: {_dersOgrencileri.Count} • Derslik: {_secilenDerslikler.Count}";
        }

        // ------------------- ÖĞRENCİLERİ DAĞIT -------------------
        private List<OturmaAtamasi> DagitOgrenciler(List<Ogrenciler> ogrenciler, List<Derslikler> derslikler)
        {
            var sonuc = new List<OturmaAtamasi>();
            int ogrIndex = 0;

            foreach (var salon in derslikler)
            {
                int rows = salon.BoyunaSira;
                int cols = salon.EnineSira;
                int kapasite = rows * cols;
                int doldurulacak = Math.Min(kapasite, ogrenciler.Count - ogrIndex);
                int sayac = 0;

                for (int r = 1; r <= rows && sayac < doldurulacak; r++)
                {
                    for (int c = 1; c <= cols && sayac < doldurulacak; c++)
                    {
                        var ogr = ogrenciler[ogrIndex++];
                        sayac++;

                        sonuc.Add(new OturmaAtamasi
                        {
                            DerslikId = salon.DerslikId,
                            DerslikAd = salon.DerslikAd,
                            SiraNo = r,
                            SutunNo = c,
                            OgrenciId = ogr.OgrenciId,
                            OgrenciNo = ogr.OgrenciNo,
                            AdSoyad = ogr.AdSoyad,
                            Grup = salon.SiraYapisi
                        });
                    }
                }

                if (ogrIndex >= ogrenciler.Count) break;
            }

            return sonuc;
        }

        // ------------------- GÖRSEL MODEL OLUŞTUR -------------------
        private List<SalonPlanVM> AtamalariGorunumeCevir(List<OturmaAtamasi> atamalar)
        {
            var list = new List<SalonPlanVM>();

            var gruplu = atamalar.GroupBy(a => new { a.DerslikId, a.DerslikAd });

            foreach (var g in gruplu)
            {
                var salon = _secilenDerslikler.First(s => s.DerslikId == g.Key.DerslikId);

                var vm = new SalonPlanVM
                {
                    DerslikId = salon.DerslikId,
                    DerslikAd = salon.DerslikAd,
                    Rows = salon.BoyunaSira,
                    Cols = salon.EnineSira,
                    Grup = salon.SiraYapisi
                };

                for (int r = 1; r <= vm.Rows; r++)
                {
                    for (int c = 1; c <= vm.Cols; c++)
                    {
                        vm.Seats.Add(new SeatCell
                        {
                            SiraNo = r,
                            SutunNo = c,
                            OgrenciNo = "",
                            AdSoyad = "",
                            IsEmpty = true
                        });
                    }
                }

                foreach (var a in g)
                {
                    var cell = vm.Seats.First(s => s.SiraNo == a.SiraNo && s.SutunNo == a.SutunNo);
                    cell.OgrenciNo = a.OgrenciNo;
                    cell.AdSoyad = a.AdSoyad;
                    cell.IsEmpty = false;
                }

                list.Add(vm);
            }

            return list;
        }

        // ------------------- SEKME GÖRSELLERİNİ ÇİZ -------------------
        private void CizSekmeler(List<SalonPlanVM> salonlar)
        {
            SalonTabs.Items.Clear();

            foreach (var salon in salonlar)
            {
                var tab = new TabItem { Header = $"{salon.DerslikAd}" };
                var uniform = new UniformGrid
                {
                    Rows = salon.Rows,
                    Columns = salon.Cols,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(10)
                };

                foreach (var cell in salon.Seats)
                {
                    var border = new Border
                    {
                        Width = 120,
                        Height = 70,
                        Margin = new Thickness(4),
                        CornerRadius = new CornerRadius(6),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.White
                    };

                    if (!cell.IsEmpty)
                    {
                        int koltukNo = (cell.SiraNo - 1) * salon.Cols + cell.SutunNo;
                        if (salon.Grup == 2 && koltukNo % 2 == 0)
                            border.Background = new SolidColorBrush(Color.FromRgb(227, 249, 253));
                        else if (salon.Grup == 3 && koltukNo % 3 == 0)
                            border.Background = new SolidColorBrush(Color.FromRgb(232, 245, 255));
                    }

                    var stack = new StackPanel { Margin = new Thickness(4) };
                    stack.Children.Add(new TextBlock { Text = $"({cell.SiraNo},{cell.SutunNo})", FontSize = 11, Foreground = Brushes.Gray });
                    stack.Children.Add(new TextBlock { Text = cell.OgrenciNo == "" ? "—" : cell.OgrenciNo, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center });
                    stack.Children.Add(new TextBlock { Text = cell.AdSoyad, FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });

                    border.Child = stack;
                    uniform.Children.Add(border);
                }

                tab.Content = new ScrollViewer { Content = uniform };
                SalonTabs.Items.Add(tab);
            }

            if (SalonTabs.Items.Count > 0)
                ((TabItem)SalonTabs.Items[0]).IsSelected = true;
        }

        // ------------------- PDF AKTARIMI -------------------
        private void PdfKaydetButton_Click(object sender, RoutedEventArgs e)
        {
            if (OturmaListesiGrid.ItemsSource == null)
            {
                MessageBox.Show("Önce oturma planını oluşturun!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "PDF Dosyası|*.pdf",
                FileName = $"{_seciliDers?.DersAdi}_OturmaPlani.pdf"
            };

            if (dlg.ShowDialog() != true) return;

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 11);

            double y = 40;
            gfx.DrawString($"Ders: {_seciliDers?.DersAdi}", font, XBrushes.Black, new XRect(40, y, page.Width, 20));
            y += 25;

            var data = OturmaListesiGrid.ItemsSource.Cast<dynamic>().ToList();
            foreach (var item in data)
            {
                string line = $"{item.OgrenciNo} - {item.AdSoyad} | {item.DerslikAd} ({item.SiraNo},{item.SutunNo})";
                gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, 20));
                y += 18;
                if (y > page.Height - 50)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            doc.Save(dlg.FileName);
            Process.Start("explorer.exe", dlg.FileName);
        }
    }

    // ------------------- YARDIMCI SINIFLAR -------------------
    public class OturmaAtamasi
    {
        public int DerslikId { get; set; }
        public string DerslikAd { get; set; } = "";
        public int SiraNo { get; set; }
        public int SutunNo { get; set; }
        public int OgrenciId { get; set; }
        public string OgrenciNo { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public byte Grup { get; set; }
    }

    public class SeatCell
    {
        public int SiraNo { get; set; }
        public int SutunNo { get; set; }
        public string OgrenciNo { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public bool IsEmpty { get; set; }
    }

    public class SalonPlanVM
    {
        public int DerslikId { get; set; }
        public string DerslikAd { get; set; } = "";
        public int Rows { get; set; }
        public int Cols { get; set; }
        public byte Grup { get; set; }
        public ObservableCollection<SeatCell> Seats { get; set; } = new();
    }
}
