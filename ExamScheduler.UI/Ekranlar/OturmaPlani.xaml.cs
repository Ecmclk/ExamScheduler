using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using Border = System.Windows.Controls.Border;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextAlignment = System.Windows.TextAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class OturmaPlani : UserControl
    {
        private readonly AppDbContext _context;
        private List<SinavKaydi> _sinavProgrami;

        public OturmaPlani(AppDbContext context)
        {
            InitializeComponent();
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void SetSinavProgrami(List<SinavKaydi> sinavProgrami)
        {
            _sinavProgrami = sinavProgrami;

            var liste = _sinavProgrami
                .GroupBy(s => new { s.DersId, s.DersAdi, s.Tarih, s.Saat })
                .Select(g => new
                {
                    g.Key.DersId,
                    g.Key.DersAdi,
                    g.Key.Tarih,
                    g.Key.Saat,
                    Salonlar = string.Join(",", g.Select(x => x.Salon).Distinct()) // tüm derslikleri virgülle ayır
                })
                .OrderBy(s => s.Tarih)
                .ThenBy(s => s.Saat)
                .ToList();

            SinavListBox.ItemsSource = liste;
        }


        private void SinavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SinavListBox.SelectedItem == null) return;

            var secilen = SinavListBox.SelectedItem;
            int dersId = (int)secilen.GetType().GetProperty("DersId").GetValue(secilen);

            // Seçilen sınav kaydı
            var secilenSinavlar = _sinavProgrami.Where(s => s.DersId == dersId).ToList();
            if (!secilenSinavlar.Any()) return;

            OturmaAlani.Children.Clear();

            // Dersliklere göre gruplama
            var derslikGruplari = secilenSinavlar.GroupBy(s => s.Salon);

            foreach (var grup in derslikGruplari)
            {
                var derslikAd = grup.Key;
                var derslik = _context.Derslikler.FirstOrDefault(d => d.DerslikAd == derslikAd);
                if (derslik == null) continue;

                // Bu derslikteki öğrenciler
                var ogrenciListesi = grup
                    .SelectMany(s => s.OgrenciIdListesi)
                    .Select(id => _context.Ogrenciler.First(o => o.OgrenciId == id))
                    .ToList();

                // Her derslik için başlık
                var baslikText = new TextBlock
                {
                    Text = $"Derslik: {derslik.DerslikAd}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 8, 0, 8),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                OturmaAlani.Children.Add(baslikText);

                // Oturma düzeni çiz
                CizOturmaDuzeni(derslik, ogrenciListesi);
            }
        }


private void CizOturmaDuzeni(Derslikler derslik, List<Ogrenciler> ogrenciListesi)
{
    if (derslik.EnineSira <= 0 || derslik.BoyunaSira <= 0)
    {
        MessageBox.Show("Geçerli sıra bilgisi bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    var mainPanel = new StackPanel
    {
        Orientation = Orientation.Vertical,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    int ogrenciIndex = 0;
    int toplamSira = derslik.BoyunaSira * derslik.EnineSira * derslik.SiraYapisi;

    // Matris: BoyunaSira x EnineSira x SiraYapisi
    bool[,,] doluMatris = new bool[derslik.BoyunaSira, derslik.EnineSira, derslik.SiraYapisi];

    for (int i = 0; i < derslik.BoyunaSira; i++)
    {
        var rowPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        for (int j = 0; j < derslik.EnineSira; j++)
        {
            var grupPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(12, 0, 12, 0)
            };

            for (int k = 0; k < derslik.SiraYapisi; k++)
            {
                var sira = new Border
                {
                    Width = 60,
                    Height = 50,
                    Margin = new Thickness(3),
                    Background = Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6)
                };

                string textStr = "";

                // Yan yana kontrol
                bool yanYanaVar = false;
                if (k > 0 && doluMatris[i, j, k - 1]) yanYanaVar = true;
                if (k == 0 && j > 0 && doluMatris[i, j - 1, derslik.SiraYapisi - 1]) yanYanaVar = true;

                // Ön-arka bağlantı kontrolü: önceki sıradaki aynı konumdaki koltuk dolu mu
                bool onArkaVar = false;
                if (i > 0)
                {
                    if (doluMatris[i - 1, j, k]) onArkaVar = true;
                }

                // Yerleşim: yan yana boş, ön-arka dolu olmalı
                if (!yanYanaVar && (i == 0 || onArkaVar) && ogrenciIndex < ogrenciListesi.Count)
                {
                    var ogr = ogrenciListesi[ogrenciIndex++];
                    textStr = $"{ogr.OgrenciNo}\n{ogr.AdSoyad}";
                    doluMatris[i, j, k] = true;
                    sira.Background = Brushes.LightBlue;
                }

                var text = new TextBlock
                {
                    Text = string.IsNullOrEmpty(textStr) ? "-" : textStr,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12
                };

                sira.Child = text;
                grupPanel.Children.Add(sira);
            }

            var grupBorder = new Border
            {
                Padding = new Thickness(6),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.LightGray,
                Background = new SolidColorBrush(Color.FromRgb(245, 250, 255)),
                Child = grupPanel
            };

            rowPanel.Children.Add(grupBorder);
        }

        mainPanel.Children.Add(rowPanel);
    }

    OturmaAlani.Children.Add(mainPanel);
}







       private void PDFIndirButton_Click(object sender, RoutedEventArgs e)
{
    if (SinavListBox.SelectedItem == null)
    {
        MessageBox.Show("Önce bir sınav seçin!");
        return;
    }

    var secilen = SinavListBox.SelectedItem;
    int dersId = (int)secilen.GetType().GetProperty("DersId").GetValue(secilen);

    var secilenSinavlar = _sinavProgrami.Where(s => s.DersId == dersId).ToList();
    if (!secilenSinavlar.Any()) return;

    var saveDialog = new SaveFileDialog
    {
        Filter = "PDF Dosyası (*.pdf)|*.pdf",
        Title = "Oturma Planını Kaydet",
        FileName = $"OturmaPlani_{dersId}.pdf"
    };

    if (saveDialog.ShowDialog() != true) return;

    string filePath = saveDialog.FileName;

    try
    {
        using (var writer = new PdfWriter(filePath))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            var derslikGruplari = secilenSinavlar.GroupBy(s => s.Salon);

            foreach (var grup in derslikGruplari)
            {
                var derslikAd = grup.Key;
                var derslik = _context.Derslikler.FirstOrDefault(d => d.DerslikAd == derslikAd);
                if (derslik == null) continue;

                var ogrenciListesi = grup
                    .SelectMany(s => s.OgrenciIdListesi)
                    .Select(id => _context.Ogrenciler.First(o => o.OgrenciId == id))
                    .ToList();

                // Başlık
                var baslik = new Paragraph($"Oturma Planı - {derslik.DerslikAd}")
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(baslik);

                // Tablo oluştur
                int toplamSutun = derslik.EnineSira * derslik.SiraYapisi;
                var table = new Table(toplamSutun, true).SetWidth(UnitValue.CreatePercentValue(100));

                int ogrenciIndex = 0;
                for (int i = 0; i < derslik.BoyunaSira; i++)
                {
                    for (int j = 0; j < derslik.EnineSira; j++)
                    {
                        for (int k = 0; k < derslik.SiraYapisi; k++)
                        {
                            Cell cell;
                            if (ogrenciIndex < ogrenciListesi.Count)
                            {
                                var ogrenci = ogrenciListesi[ogrenciIndex++];
                                cell = new Cell().Add(new Paragraph($"{ogrenci.OgrenciNo}\n{ogrenci.AdSoyad}"));
                            }
                            else
                            {
                                cell = new Cell().Add(new Paragraph("-"));
                            }

                            cell.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .SetPadding(5)
                                .SetBorder(new SolidBorder(1));

                            table.AddCell(cell);
                        }
                    }
                }

                document.Add(table);
                document.Add(new Paragraph("\n")); // Derslikler arasında boşluk
            }

            document.Close();
        }

        MessageBox.Show("PDF başarıyla oluşturuldu!");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"PDF oluşturulurken hata oluştu: {ex.Message}");
    }
}

    }

    
}
