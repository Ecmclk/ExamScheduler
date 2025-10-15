using ExamScheduler.Models;
using ExamScheduler.Models.Models;
using System.Collections.Generic;
using System.Windows;

// DİKKAT: namespace, dosyanın 'Ekranlar' klasöründe olduğunu belirtiyor
namespace ExamScheduler.UI.Ekranlar
{
    public partial class MainPanelWindow : Window
    {
        private readonly Kullanicilar _currentUser;

        // Constructor, giriş yapan kullanıcıyı kabul edecek şekilde hazırlandı.
        public MainPanelWindow(Kullanicilar user)
        {
            InitializeComponent();
            _currentUser = user;

            // TODO: Rol tabanlı yetkilendirme ve veri yükleme kodları buraya gelecek.
        }
    }
}