using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamScheduler.UI.Ekranlar
{
    public partial class DerslikYonetimi : UserControl
    {
        private readonly AppDbContext _context;
        private Derslikler _duzenlenen; // düzenleme modunda tutulan kayıt
        private int _koordinatorBolumId; // giriş yapan koordinatörün bölümü

        public DerslikYonetimi()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _koordinatorBolumId = ResolveKoordinatorBolumId();
            LoadDerslikler(); // sadece koordinatörün bölümündekiler
        }

        // Koordinatörün BolumId’sini otomatık çöz.
        // Uygulamada CurrentUser nesnesini nerede tuttuğuna göre bu metodu uyarlayabilirsin.
        private int ResolveKoordinatorBolumId()
        {
            try
            {
                // Örnek 1: Application.Current.Properties üzerinden
                if (Application.Current?.Properties != null &&
                    Application.Current.Properties.Contains("CurrentUser") &&
                    Application.Current.Properties["CurrentUser"] is Kullanicilar k1 &&
                    k1.BolumId.HasValue)
                {
                    return k1.BolumId.Value;
                }

                // Örnek 2: Ana pencerede CurrentUser property’si varsa
                var win = Application.Current?.MainWindow as Window;
                var mainPanelType = win?.GetType();
                var prop = mainPanelType?.GetProperty("CurrentUser");
                if (prop != null)
                {
                    var k2 = prop.GetValue(win) as Kullanicilar;
                    if (k2?.BolumId != null) return k2.BolumId.Value;
                }
            }
            catch { /* yumuşak düş */ }

            // Son çare: 1. bölüm varsayımı (geliştirirken boş geçmesin diye).
            return 1;
        }

        // Liste yükleme (+ opsiyonel arama)
        private void LoadDerslikler(string arama = null)
        {
            var q = _context.Derslikler
                            .Include(d => d.Bolum)
                            .Where(d => d.BolumId == _koordinatorBolumId);

            if (!string.IsNullOrWhiteSpace(arama))
            {
                var s = arama.Trim().ToLower();
                q = q.Where(d => d.DerslikAd.ToLower().Contains(s) ||
                                 d.DerslikKod.ToLower().Contains(s));
            }

            DersliklerDataGrid.ItemsSource = q
                .OrderBy(d => d.DerslikAd)
                .ToList();
        }

        // Kaydet: yeni kayıt ekleme
        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_duzenlenen != null)
            {
                MessageBox.Show("Düzenleme modundasın. Lütfen 'Güncelle'yi kullan ya da 'İptal' et.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!TryReadForm(out var model, out string msg))
            {
                MessageBox.Show(msg, "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.Derslikler.Add(model);
                _context.SaveChanges();
                MessageBox.Show("Derslik başarıyla eklendi.", "Bilgi",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                TemizleForm();
                LoadDerslikler(AramaTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme sırasında hata: {ex.Message}", "Hata",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Düzenle (satırdaki buton)
        private void DuzenleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                _duzenlenen = _context.Derslikler.FirstOrDefault(d => d.DerslikId == id);
                if (_duzenlenen == null)
                {
                    MessageBox.Show("Derslik bulunamadı.", "Hata",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Formu doldur
                DerslikKoduTextBox.Text = _duzenlenen.DerslikKod;
                DerslikAdTextBox.Text = _duzenlenen.DerslikAd;
                KapasiteTextBox.Text = _duzenlenen.Kapasite.ToString();
                EnineSiraTextBox.Text = _duzenlenen.EnineSira.ToString();
                BoyunaSiraTextBox.Text = _duzenlenen.BoyunaSira.ToString();

                // SıraYapısı combobox seçimi
                var selected = SiraYapisiComboBox.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(ci => ci.Content?.ToString() == _duzenlenen.SiraYapisi.ToString());
                SiraYapisiComboBox.SelectedItem = selected;

                // Buton modları
                GuncelleButton.Visibility = Visibility.Visible;
                IptalButton.Visibility = Visibility.Visible;
                EkleButton.Visibility = Visibility.Collapsed;
            }
        }

        // Güncelle
        private void GuncelleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_duzenlenen == null)
            {
                MessageBox.Show("Düzenlenecek bir derslik seçilmedi.", "Uyarı",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryReadForm(out var model, out string msg))
            {
                MessageBox.Show(msg, "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Sadece değişen alanları aktar
                _duzenlenen.DerslikKod = model.DerslikKod;
                _duzenlenen.DerslikAd = model.DerslikAd;
                _duzenlenen.Kapasite = model.Kapasite;
                _duzenlenen.EnineSira = model.EnineSira;
                _duzenlenen.BoyunaSira = model.BoyunaSira;
                _duzenlenen.SiraYapisi = model.SiraYapisi;

                _context.SaveChanges();
                MessageBox.Show("Derslik güncellendi.", "Bilgi",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                TemizleForm();
                LoadDerslikler(AramaTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme sırasında hata: {ex.Message}", "Hata",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // İptal
        private void IptalButton_Click(object sender, RoutedEventArgs e)
        {
            TemizleForm();
        }

        // Sil (satırdaki buton)
        private void SilButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var entity = _context.Derslikler.FirstOrDefault(d => d.DerslikId == id);
                if (entity == null)
                {
                    MessageBox.Show("Derslik bulunamadı.", "Hata",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Bu dersliği silmek istiyor musunuz?", "Onay",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Derslikler.Remove(entity);
                        _context.SaveChanges();
                        LoadDerslikler(AramaTextBox.Text);
                        MessageBox.Show("Derslik silindi.", "Bilgi",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Silme sırasında hata: {ex.Message}", "Hata",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Ara
        private void AraButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDerslikler(AramaTextBox.Text);
        }

        // Tümünü göster
        private void TumunuGosterButton_Click(object sender, RoutedEventArgs e)
        {
            AramaTextBox.Clear();
            LoadDerslikler();
        }

        // Satıra çift tıklama: Derslik görselini ayrı pencerede aç
        private void DersliklerDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DersliklerDataGrid.SelectedItem is Derslikler secili)
            {
                var owner = Window.GetWindow(this);
                var pencere = new DerslikGorsel(secili) // <<— Window adı net: DerslikGorsel
                {
                    Owner = owner
                };
                pencere.ShowDialog();
            }
        }

        // Formu oku & doğrula — model dön
        private bool TryReadForm(out Derslikler model, out string mesaj)
        {
            model = null;
            mesaj = null;

            string kod = DerslikKoduTextBox.Text?.Trim();
            string ad = DerslikAdTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(kod) || string.IsNullOrWhiteSpace(ad))
            {
                mesaj = "Derslik kodu ve adı zorunludur.";
                return false;
            }

            if (!int.TryParse(KapasiteTextBox.Text, out int kapasite) || kapasite <= 0)
            {
                mesaj = "Kapasite pozitif bir sayı olmalıdır.";
                return false;
            }
            if (!int.TryParse(EnineSiraTextBox.Text, out int enine) || enine <= 0)
            {
                mesaj = "Enine sıra sayısı pozitif bir sayı olmalıdır.";
                return false;
            }
            if (!int.TryParse(BoyunaSiraTextBox.Text, out int boyuna) || boyuna <= 0)
            {
                mesaj = "Boyuna sıra sayısı pozitif bir sayı olmalıdır.";
                return false;
            }

            if (SiraYapisiComboBox.SelectedItem is not ComboBoxItem cb ||
                !byte.TryParse(cb.Content?.ToString(), out byte siraYapisi) ||
                (siraYapisi != 2 && siraYapisi != 3))
            {
                mesaj = "Sıra yapısı 2 veya 3 olmalıdır.";
                return false;
            }

            model = new Derslikler
            {
                BolumId = _koordinatorBolumId,
                DerslikKod = kod,
                DerslikAd = ad,
                Kapasite = kapasite,
                EnineSira = enine,
                BoyunaSira = boyuna,
                SiraYapisi = siraYapisi
            };
            return true;
        }

        // Formu sıfırla & moddan çık
        private void TemizleForm()
        {
            DerslikKoduTextBox.Clear();
            DerslikAdTextBox.Clear();
            KapasiteTextBox.Clear();
            EnineSiraTextBox.Clear();
            BoyunaSiraTextBox.Clear();
            SiraYapisiComboBox.SelectedIndex = -1;

            _duzenlenen = null;
            GuncelleButton.Visibility = Visibility.Collapsed;
            IptalButton.Visibility = Visibility.Collapsed;
            EkleButton.Visibility = Visibility.Visible;
        }
    }
}
