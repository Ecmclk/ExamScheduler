using ClosedXML.Excel;
using ExamScheduler.Core.Interfaces;
using ExamScheduler.Models.Models;

namespace ExamScheduler.Data.Parsers
{
    /// <summary>
    /// Ders Listesi Yükleme İşlemi
    /// Yetki: Yalnızca Bölüm Koordinatörü erişebilir.
    /// ⚠️ NOT: Yetki kontrolü UI veya Controller katmanında yapılmalıdır.
    /// </summary>
    public class DersListesiExcelParser : IExcelParser<Dersler>
    {
        public async Task<ParseResult<Dersler>> ParseAsync(string filePath)
        {
            var result = new ParseResult<Dersler>();

            if (!File.Exists(filePath))
            {
                result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                {
                    RowNumber = 0,
                    Message = $"Dosya bulunamadı: {filePath}"
                });
                return await Task.FromResult(result);
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);

                foreach (var worksheet in workbook.Worksheets)
                {
                    int currentClassLevel = 0;
                    bool isElective = false;

                    foreach (var row in worksheet.RowsUsed())
                    {
                        string cell1 = row.Cell(1).GetString().Trim();
                        string cell2 = row.Cell(2).GetString().Trim();

                        // Boş satırları atla
                        if (string.IsNullOrWhiteSpace(cell1) && string.IsNullOrWhiteSpace(cell2))
                            continue;

                        // Sınıf başlıklarını kontrol et
                        if (cell1.Contains("Sınıf", StringComparison.OrdinalIgnoreCase))
                        {
                            currentClassLevel = cell1.StartsWith("1") ? 1 :
                                                cell1.StartsWith("2") ? 2 :
                                                cell1.StartsWith("3") ? 3 :
                                                cell1.StartsWith("4") ? 4 : 0;

                            isElective = false;
                            continue;
                        }

                        // Seçmeli ders kontrolü
                        if (cell1.Contains("SEÇMEL", StringComparison.OrdinalIgnoreCase) ||
                            cell1.Contains("SEÇİML", StringComparison.OrdinalIgnoreCase) ||
                            cell2.Contains("SEÇMEL", StringComparison.OrdinalIgnoreCase) ||
                            cell2.Contains("SEÇİML", StringComparison.OrdinalIgnoreCase))
                        {
                            isElective = true;
                            if (string.IsNullOrWhiteSpace(cell2))
                                continue;
                        }

                        // Başlık satırlarını atla
                        if (cell1.ToUpper().Contains("DERS KODU"))
                            continue;

                        // Ders adı boşsa atla
                        if (string.IsNullOrWhiteSpace(cell2))
                            continue;

                        try
                        {
                            var dersKodu = cell1;
                            var dersAdi = cell2;
                            var ogretimGorevlisi = row.Cell(3).GetString().Trim();

                            if (string.IsNullOrEmpty(dersKodu))
                                throw new Exception("Ders kodu boş olamaz.");
                            if (string.IsNullOrEmpty(dersAdi))
                                throw new Exception("Ders adı boş olamaz.");

                            // Eksik öğretim görevlisi hatası
                            if (string.IsNullOrEmpty(ogretimGorevlisi))
                            {
                                result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                                {
                                    RowNumber = row.RowNumber(),
                                    Message = $"[{worksheet.Name}] Satır {row.RowNumber()}: Öğretim görevlisi bilgisi eksik (Ders Kodu: {dersKodu})"
                                });
                            }

                            // Veriyi ParseResult’e ekle
                            result.Data.Add(new Dersler
                            {
                                DersKodu = dersKodu,
                                DersAdi = dersAdi,
                                OgretimGorevlisi = ogretimGorevlisi,
                                Sinif = (byte)currentClassLevel,
                                DersTuru = isElective ? "Seçmeli" : "Zorunlu",
                                BolumId = 1
                            });
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                            {
                                RowNumber = row.RowNumber(),
                                Message = $"[{worksheet.Name}] Satır {row.RowNumber()}: {ex.Message}"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                {
                    RowNumber = 0,
                    Message = $"Dosya okunurken hata oluştu: {ex.Message}"
                });
            }

            return await Task.FromResult(result);
        }
    }
}
