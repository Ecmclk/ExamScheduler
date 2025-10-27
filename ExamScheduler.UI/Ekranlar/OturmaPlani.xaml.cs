using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using iText.IO.Font;

using iText.IO.Image;

using Microsoft.Win32;
using Border = System.Windows.Controls.Border;
using Color = System.Windows.Media.Color;
using FontWeights = System.Windows.FontWeights;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = iText.Layout.Element.Image;
using TextAlignment = System.Windows.TextAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

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

            var secilenSinavlar = _sinavProgrami.Where(s => s.DersId == dersId).ToList();
            if (!secilenSinavlar.Any()) return;

            // Derslik butonlarını temizle
            DerslikButonPanel.Children.Clear();
            OturmaAlani.Children.Clear();

            // Derslikler
            var derslikGruplari = secilenSinavlar.GroupBy(s => s.Salon);

            foreach (var grup in derslikGruplari)
            {
                var derslikAd = grup.Key;
                var derslik = _context.Derslikler.FirstOrDefault(d => d.DerslikAd == derslikAd);
                if (derslik == null) continue;

                // Buton oluştur
                var btn = new Button
                {
                    Content = derslikAd,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = new { Derslik = derslik, Sinavlar = grup.ToList() }
                };
                
                


                btn.Click += DerslikButton_Click;
                DerslikButonPanel.Children.Add(btn);
            }
        }
        private void DerslikButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            // Tag aslında bir anonim tip olacak şekilde atanmalı
            // Örn: btn.Tag = new { Derslik = derslik, Sinavlar = grup.ToList() };
            var tagType = btn.Tag.GetType();

            var derslik = tagType.GetProperty("Derslik")?.GetValue(btn.Tag) as Derslikler;
            var sinavlar = tagType.GetProperty("Sinavlar")?.GetValue(btn.Tag) as List<SinavKaydi>;

            if (derslik == null || sinavlar == null) return;

            OturmaAlani.Children.Clear();

            var ogrenciler = sinavlar
                .SelectMany(s => s.OgrenciIdListesi)
                .Select(id => _context.Ogrenciler.FirstOrDefault(o => o.OgrenciId == id))
                .Where(o => o != null)
                .ToList();

            OturmaAlani.Children.Add(new TextBlock
            {
                Text = $"Derslik: {derslik.DerslikAd}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 8, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            CizOturmaDuzeni(derslik, ogrenciler);

            PDFIndirButton.Visibility = Visibility.Visible;
            PDFIndirButton.Tag = new { Derslik = derslik, Ogrenciler = ogrenciler };
        }




        private void CizOturmaDuzeni(Derslikler derslik, List<Ogrenciler> ogrenciListesi)
        {
            if (derslik.EnineSira <= 0 || derslik.BoyunaSira <= 0)
            {
                MessageBox.Show("Geçerli sıra bilgisi bulunamadı.", "Uyarı", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
        if (PDFIndirButton.Tag == null) return;

        dynamic tagData = PDFIndirButton.Tag;
        var derslik = tagData.Derslik as Derslikler;
        var ogrenciler = tagData.Ogrenciler as List<Ogrenciler>;
        var secilenDers = SinavListBox.SelectedItem;
        string dersAdi = secilenDers.GetType().GetProperty("DersAdi").GetValue(secilenDers)?.ToString();
        
        var saveFileDialog = new SaveFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Filter = "PDF Dosyası (*.pdf)|*.pdf",
            FileName = $"OturmaPlani_{dersAdi}_{derslik.DerslikAd}.pdf"
        };

        if (saveFileDialog.ShowDialog() != true) return;
       
        // Font dosya yolu
        string fontPath = @"C:\Users\Ecem\RiderProjects\ExamScheduler\Font\NotoSans-VariableFont_wdth,wght.ttf";
        var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

        
      
        
        using (var writer = new PdfWriter(saveFileDialog.FileName))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

            document.Add(new Paragraph($"🪑 Oturma Planı - {derslik.DerslikAd}")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetFontSize(18)
                .SetFont(boldFont)
                .SetMarginBottom(15));

            // UI görselini PDF'e ekle
            if (OturmaAlani.ActualWidth > 0 && OturmaAlani.ActualHeight > 0)
            {
                OturmaAlani.UpdateLayout();
                OturmaAlani.Measure(new Size(OturmaAlani.ActualWidth, OturmaAlani.ActualHeight));
                OturmaAlani.Arrange(new Rect(new Size(OturmaAlani.ActualWidth, OturmaAlani.ActualHeight)));

                int w = (int)(OturmaAlani.ActualWidth * 2);
                int h = (int)(OturmaAlani.ActualHeight * 2);

                RenderTargetBitmap rtb = new RenderTargetBitmap(w, h, 192, 192, PixelFormats.Pbgra32);
                rtb.Render(OturmaAlani);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                string tempFile = Path.GetTempFileName() + ".png";
                using (var fs = new FileStream(tempFile, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                var pdfImage = new Image(ImageDataFactory.Create(tempFile))
                    .SetWidth(pdf.GetDefaultPageSize().GetWidth() - 40)
                    .SetAutoScaleHeight(true)
                    .SetMarginTop(5);

                document.Add(pdfImage);
                document.Add(new Paragraph("\n"));
            }

            // Öğrenci tablosu ekle (CizOturmaDuzeni ile aynı düzeni kullan)
            var table = new Table(5).UseAllAvailableWidth();
            table.AddHeaderCell("No");
            table.AddHeaderCell("Öğrenci No");
            table.AddHeaderCell("Ad Soyad");
            table.AddHeaderCell("Sıra");
            table.AddHeaderCell("Sütun");

            int ogrenciIndex = 0;
            int genelSayac = 1;
            bool[,,] doluMatris = new bool[derslik.BoyunaSira, derslik.EnineSira, derslik.SiraYapisi];
            
        



            for (int i = 0; i < derslik.BoyunaSira; i++)
            {
                for (int j = 0; j < derslik.EnineSira; j++)
                {
                    for (int k = 0; k < derslik.SiraYapisi; k++)
                    {
                        bool yanYanaVar = false;
                        if (k > 0 && doluMatris[i, j, k - 1]) yanYanaVar = true;
                        if (k == 0 && j > 0 && doluMatris[i, j - 1, derslik.SiraYapisi - 1]) yanYanaVar = true;

                        bool onArkaVar = false;
                        if (i > 0 && doluMatris[i - 1, j, k]) onArkaVar = true;

                        if (!yanYanaVar && (i == 0 || onArkaVar) && ogrenciIndex < ogrenciler.Count)
                        {
                            var ogr = ogrenciler[ogrenciIndex++];
                            doluMatris[i, j, k] = true;

                            table.AddCell(new Cell().Add(new Paragraph(genelSayac.ToString())).SetFont(font));
                            table.AddCell(new Cell().Add(new Paragraph(ogr.OgrenciNo ?? "-")).SetFont(font));
                            table.AddCell(new Cell().Add(new Paragraph(ogr.AdSoyad ?? "-")).SetFont(font));
                            table.AddCell(new Cell().Add(new Paragraph($"Sıra {i + 1}")).SetFont(font));
                            table.AddCell(new Cell().Add(new Paragraph($"Sütun {j + 1}")).SetFont(font));

                            genelSayac++;
                        }
                    }
                }
            }

            document.Add(table);
            document.Close();
        }

        MessageBox.Show("Oturma planı PDF olarak başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
}


   
    
    




    }
}
