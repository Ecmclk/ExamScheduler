using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using ExamScheduler.Services;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class SinavProgramiOlustur : UserControl
    {
        private readonly DersService _dersService;

        public SinavProgramiOlustur(DersService dersService)
        {
            InitializeComponent();
            _dersService = dersService;
            LoadDersler();
        }

        private async void LoadDersler()
        {
            var dersler = await _dersService.GetAllDerslerAsync(); // veritabanından dersleri çekin

            // Her derse Secili property’si ekleyin (view model için)
            var dersViewModel = dersler.Select(d => new Dersler
            {
                DersAdi = d.DersAdi,
                Secili = true
            }).ToList();

            DersListBox.ItemsSource = dersViewModel;
        }


        private async void ProgramiOlusturButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Seçili dersleri al
                var secilenDersler = DersListBox.Items.Cast<Dersler>()
                    .Where(d => d.Secili)
                    .ToList();

                if (!secilenDersler.Any())
                {
                    MessageBox.Show("En az bir ders seçmelisiniz!", "Uyarı", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Tarih aralığı ve kısıtlar
                DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Now.Date;
                DateTime endDate = EndDatePicker.SelectedDate ?? startDate.AddDays(7);
                var haricGunler = HaricTutulacakTarihlerCalendar.SelectedDates;
                bool haftaSonuKisit = CumartesiPazarKisit.IsChecked == true;
             
                int beklemeSuresi = int.TryParse(BeklemeSuresiTextBox.Text, out var b) ? b : 15;
                string sinavTuru = ((ComboBoxItem)SinavTuruComboBox.SelectedItem)?.Content.ToString() ?? "Final";

                var examList = new List<object>();
                int dersIndex = 0;

                // Tarihleri sırayla gez
                for (DateTime date = startDate; date <= endDate && dersIndex < secilenDersler.Count; date = date.AddDays(1))
                {
                    if (haftaSonuKisit && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                        continue;

                    if (haricGunler.Contains(date))
                        continue;

                    // Günlük 3 ders sınavı
                    int gunlukBaslangicSaati = 9;

                    for (int i = 0; i < 3 && dersIndex < secilenDersler.Count; i++)
                    {
                        var ders = secilenDersler[dersIndex++];
                        int sure = ders.SinavSuresi > 0 ? ders.SinavSuresi : 75;

                        examList.Add(new
                        {
                            DersAdi = ders.DersAdi,
                            Tarih = date.ToShortDateString(),
                            Saat = $"{gunlukBaslangicSaati}:00",
                            Salon = $"Derslik {dersIndex}"
                        });

                        // Sonraki sınavın başlangıcı = mevcut sınav süresi + bekleme süresi
                        gunlukBaslangicSaati += (sure + beklemeSuresi) / 60;
                    }
                }

                // Sonuçları DataGrid'e aktar
                SinavProgramiDataGrid.ItemsSource = examList;
                MessageBox.Show("Sınav programı başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Program oluşturulurken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
