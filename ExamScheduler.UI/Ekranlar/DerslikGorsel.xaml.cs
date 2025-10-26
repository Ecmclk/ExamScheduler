using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class DerslikGorsel : Window
    {
        private readonly Derslikler _derslik;

        public DerslikGorsel(Derslikler seciliDerslik)
        {
            InitializeComponent();
            _derslik = seciliDerslik;
            YukleDerslikBilgileri();
            CizOturmaDuzeni();
        }

        private void YukleDerslikBilgileri()
        {
            DerslikAdText.Text = _derslik.DerslikAd;
            KapasiteText.Text = _derslik.Kapasite.ToString();
        }

        private void CizOturmaDuzeni()
        {
            OturmaAlani.Children.Clear();

            if (_derslik.EnineSira <= 0 || _derslik.BoyunaSira <= 0)
            {
                MessageBox.Show("Geçerli sıra bilgisi bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OturmaAlani.Rows = _derslik.BoyunaSira;
            OturmaAlani.Columns = _derslik.EnineSira;

            int koltukNo = 1;

            for (int i = 0; i < _derslik.BoyunaSira; i++)
            {
                for (int j = 0; j < _derslik.EnineSira; j++)
                {
                    var sira = new Border
                    {
                        Width = 70,
                        Height = 55,
                        Margin = new Thickness(5),
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6)
                    };

                    var text = new TextBlock
                    {
                        Text = koltukNo.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.SemiBold
                    };

                    sira.Child = text;

                    // 2’li veya 3’lü sıralar için renk gruplaması
                    if (_derslik.SiraYapisi == 2 && koltukNo % 2 == 0)
                        sira.Background = new SolidColorBrush(Color.FromRgb(227, 249, 253));
                    else if (_derslik.SiraYapisi == 3 && koltukNo % 3 == 0)
                        sira.Background = new SolidColorBrush(Color.FromRgb(232, 245, 255));

                    OturmaAlani.Children.Add(sira);
                    koltukNo++;
                }
            }
        }
    }
}
