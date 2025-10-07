using ExamScheduler.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExamScheduler.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                VeriTabaniBaglanti db = new VeriTabaniBaglanti();

                using (SqlConnection conn = db.BaglantiAc())
                {
                    MessageBox.Show("✅ Veritabanı bağlantısı başarılı!", "Bağlantı Testi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Hata: {ex.Message}", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}