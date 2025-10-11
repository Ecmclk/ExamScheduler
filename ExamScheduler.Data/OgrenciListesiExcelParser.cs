using ClosedXML.Excel;
using ExamScheduler.Core.Interfaces;
using ExamScheduler.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Regex için bu satırı ekleyin

namespace ExamScheduler.Data
{
    public class OgrenciListesiExcelParser : IExcelParser<OgrenciDersler>
    {
        public async Task<ParseResult<OgrenciDersler>> ParseAsync(string filePath)
        {
            var result = new ParseResult<OgrenciDersler>();
            var uniqueStudents = new Dictionary<string, Ogrenciler>();

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    try
                    {
                        string ogrNo = row.Cell(1).GetString().Trim();
                        string adSoyad = row.Cell(2).GetString().Trim();
                        string sinifStr = row.Cell(3).GetString().Trim();
                        string dersKodu = row.Cell(4).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(ogrNo) || string.IsNullOrWhiteSpace(dersKodu))
                        {
                            continue;
                        }

                        if (!uniqueStudents.TryGetValue(ogrNo, out var ogrenci))
                        {
                            // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---
                            // "1.sınıf" gibi bir metinden sadece sayıyı çekmek için.
                            var sayiEslesmesi = Regex.Match(sinifStr, @"\d+");
                            string sinifSayiStr = sayiEslesmesi.Success ? sayiEslesmesi.Value : sinifStr;
                            // --- DEĞİŞİKLİK BURADA BİTİYOR ---

                            if (!byte.TryParse(sinifSayiStr, out byte sinif))
                            {
                                result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                                {
                                    RowNumber = row.RowNumber(),
                                    Message = $"'{sinifStr}' geçerli bir sınıf değeri değil."
                                });
                                continue;
                            }

                            ogrenci = new Ogrenciler
                            {
                                OgrenciNo = ogrNo,
                                AdSoyad = adSoyad,
                                Sinif = sinif,
                                BolumId = 1
                            };
                            uniqueStudents.Add(ogrNo, ogrenci);
                        }

                        var ogrDers = new OgrenciDersler
                        {
                            Ogrenci = ogrenci,
                            Ders = new Dersler { DersKodu = dersKodu }
                        };
                        result.Data.Add(ogrDers);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                        {
                            RowNumber = row.RowNumber(),
                            Message = $"Satır işlenirken bir hata oluştu: {ex.Message}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                {
                    RowNumber = 0,
                    Message = $"Excel dosyası okunurken genel bir hata oluştu: {ex.Message}"
                });
            }

            return await Task.FromResult(result);
        }
    }
}