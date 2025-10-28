using System.Text.RegularExpressions;
using ClosedXML.Excel;
using ExamScheduler.Core.Interfaces;
using ExamScheduler.Models.Models;

// Regex için bu satırı ekleyin

namespace ExamScheduler.Data
{
    public class OgrenciListesiExcelParser : IExcelParser<OgrenciDersler>
    {  
        public async Task<ParseResult<OgrenciDersler>> ParseAsync(string filePath, int bolumId)
        {
            var result = new ParseResult<OgrenciDersler>();
            var uniqueStudents = new Dictionary<string, Ogrenciler>();

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                string sheetName = worksheet.Name;

                // Başlık satırını al ve küçük harfe çevir
                var headers = worksheet.Row(1).Cells().Select(c => c.GetString().Trim().ToLower()).ToList();

                // Esnek eşleştirme
                int ogrNoIndex = headers.FindIndex(h => h.Contains("öğrenci") && h.Contains("no"));
                int adSoyadIndex = headers.FindIndex(h => h.Contains("ad") && h.Contains("soyad"));
                int sinifIndex = headers.FindIndex(h => h.Contains("sınıf"));
                int dersIndex = headers.FindIndex(h => h.Contains("ders")); 

                if (ogrNoIndex == -1 || adSoyadIndex == -1 || sinifIndex == -1 || dersIndex == -1)
                {
                    result.Errors.Add(new Core.Interfaces.ParseError
                    {
                        RowNumber = 1,
                        Message = $"[{sheetName}] Başlık satırında gerekli sütunlar bulunamadı."
                    });
                    return result;
                }

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    try
                    {
                        string ogrNo = row.Cell(ogrNoIndex + 1).GetString().Trim();
                        string adSoyad = row.Cell(adSoyadIndex + 1).GetString().Trim();
                        string sinifStr = row.Cell(sinifIndex + 1).GetString().Trim();
                        string dersKodu = row.Cell(dersIndex + 1).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(ogrNo) || string.IsNullOrWhiteSpace(dersKodu))
                        {
                            result.Errors.Add(new Core.Interfaces.ParseError
                            {
                                RowNumber = row.RowNumber(),
                                Message = $"[{sheetName}] Satır {row.RowNumber()}: Öğrenci numarası veya ders kodu boş olamaz."
                            });
                            continue;
                        }

                        var sayiEslesmesi = Regex.Match(sinifStr, @"\d+");
                        string sinifSayiStr = sayiEslesmesi.Success ? sayiEslesmesi.Value : sinifStr;

                        if (!byte.TryParse(sinifSayiStr, out byte sinif))
                        {
                            result.Errors.Add(new Core.Interfaces.ParseError
                            {
                                RowNumber = row.RowNumber(),
                                Message = $"[{sheetName}] Satır {row.RowNumber()}: '{sinifStr}' geçerli bir sınıf değeri değil."
                            });
                            continue;
                        }

                        if (!uniqueStudents.TryGetValue(ogrNo, out var ogrenci))
                        {
                            ogrenci = new Ogrenciler
                            {
                                OgrenciNo = ogrNo,
                                AdSoyad = adSoyad,
                                Sinif = sinif,
                                BolumId = bolumId
                            };
                            uniqueStudents.Add(ogrNo, ogrenci);
                        }

                        result.Data.Add(new OgrenciDersler
                        {
                            Ogrenci = ogrenci,
                            Ders = new Dersler { DersKodu = dersKodu }
                        });
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new Core.Interfaces.ParseError
                        {
                            RowNumber = row.RowNumber(),
                            Message = $"[{sheetName}] Satır {row.RowNumber()} işlenirken hata: {ex.Message}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Core.Interfaces.ParseError
                {
                    RowNumber = 0,
                    Message = $"Excel dosyası okunamadı ({Path.GetFileName(filePath)}): {ex.Message}"
                });
            }

            return await Task.FromResult(result);
        }
    }
    
}