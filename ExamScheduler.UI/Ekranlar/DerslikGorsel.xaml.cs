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

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        int koltukNo = 1; // Koltuk numarası her sırada artacak

        for (int i = 0; i < _derslik.BoyunaSira; i++)
        {
            var rowPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            for (int j = 0; j < _derslik.EnineSira; j++)
            {
                var grupPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(12, 0, 12, 0)
                };

                // Her blokta SiraYapisi kadar koltuk
                for (int k = 0; k < _derslik.SiraYapisi; k++)
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

                    var text = new TextBlock
                    {
                        Text = koltukNo.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.SemiBold
                    };

                    sira.Child = text;
                    grupPanel.Children.Add(sira);

                    koltukNo++;
                }

                var grupBorder = new Border
                {
                    Padding = new Thickness(6),
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.LightGray,
                    Background = _derslik.SiraYapisi == 2 ?
                        new SolidColorBrush(Color.FromRgb(235, 248, 255)) :
                        new SolidColorBrush(Color.FromRgb(245, 250, 255)),
                    Child = grupPanel
                };

                rowPanel.Children.Add(grupBorder);
            }

            mainPanel.Children.Add(rowPanel);
        }

        OturmaAlani.Children.Add(mainPanel);
    }






    }
}