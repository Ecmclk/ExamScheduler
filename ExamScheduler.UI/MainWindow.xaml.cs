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
            TestEt(); // Uygulama açılırken bağlantıyı test eder
        }

        private void TestEt()
        {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Hata: {ex.Message}",
                                "Bağlantı Hatası",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
