using ClosedXML.Excel;
using ExamScheduler.Core;
using ExamScheduler.Core.Interfaces;
using ExamScheduler.Models.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
/// <summary>
/// Ders Listesi Yükleme İşlemi
/// Yetki: Yalnızca Bölüm Koordinatörü erişebilir.
/// ⚠️ NOT:
/// Bu sınıfta doğrudan rol kontrolü yapılmaz.
/// Yetki kontrolü UI veya Controller katmanında yapılmalıdır.
/// (Örnek: giriş yapan kullanıcının Rol == "Koordinatör" olup olmadığı doğrulanmalı)
/// </summary>
namespace ExamScheduler.Data.Parsers
{
    public class DersListesiExcelParser : IExcelParser<Dersler>
    {
        public async Task<ParseResult<Dersler>> ParseAsync(string filePath)
        {
            var result = new ParseResult<Dersler>();

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            int currentClassLevel = 0;
            bool isElective = false;

            foreach (var row in worksheet.RowsUsed())
            {
                var cell1 = row.Cell(1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(cell1)) continue;

                if (cell1.Contains("Sınıf"))
                {
                    if (cell1.StartsWith("1")) currentClassLevel = 1;
                    else if (cell1.StartsWith("2")) currentClassLevel = 2;
                    else if (cell1.StartsWith("3")) currentClassLevel = 3;
                    else if (cell1.StartsWith("4")) currentClassLevel = 4;

                    isElective = false;
                    continue;
                }

                if (cell1.ToUpper().Contains("SEÇMELİ")) { isElective = true; continue; }
                if (cell1.ToUpper().Contains("DERS KODU")) continue;

                try
                {
                    var dersKodu = row.Cell(1).GetString().Trim();
                    var dersAdi = row.Cell(2).GetString().Trim();
                    var ogretimGorevlisi = row.Cell(3).GetString().Trim();

                    if (string.IsNullOrEmpty(dersKodu) || string.IsNullOrEmpty(dersAdi)) continue;

                    var course = new Dersler
                    {
                        DersKodu = dersKodu,
                        DersAdi = dersAdi,
                        OgretimGorevlisi = ogretimGorevlisi,
                        Sinif = (byte)currentClassLevel,
                        DersTuru = isElective ? "Seçmeli" : "Zorunlu",
                        BolumId = 1 // Daha sonra set edilecek,loginden gelen bilgi kullanılabilir.
                    };

                    result.Data.Add(course);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ExamScheduler.Core.Interfaces.ParseError
                    {
                        RowNumber = row.RowNumber(),
                        Message = ex.Message
                    });
                }
            }

            return await Task.FromResult(result);
        }
    }
}
