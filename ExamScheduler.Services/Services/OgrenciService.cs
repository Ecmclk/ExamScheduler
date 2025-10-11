

using ExamScheduler.Core.Interfaces;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Services; 

public class OgrenciService
{
    private readonly AppDbContext _dbContext;

        public OgrenciService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaveResult> SaveOgrenciDerslerAsync(ParseResult<OgrenciDersler> parseResult)
        {
            var result = new SaveResult();

            foreach (var ogrenciDers in parseResult.Data)
            {
                try
                {
                    // Mevcut öğrenci varsa DB'den al, yoksa ekle
                    var existingOgrenci = await _dbContext.Ogrenciler
                        .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciDers.Ogrenci.OgrenciNo);

                    if (existingOgrenci == null)
                    {
                        existingOgrenci = ogrenciDers.Ogrenci;
                        _dbContext.Ogrenciler.Add(existingOgrenci);
                    }

                    // Mevcut ders varsa DB'den al, yoksa ekle
                    var existingDers = await _dbContext.Dersler
                        .FirstOrDefaultAsync(d => d.DersKodu == ogrenciDers.Ders.DersKodu);

                    if (existingDers == null)
                    {
                        existingDers = ogrenciDers.Ders;
                        _dbContext.Dersler.Add(existingDers);
                    }

                    // OgrenciDersler ilişki tablosuna ekle
                    if (!await _dbContext.OgrenciDersler
                            .AnyAsync(od => od.Ogrenci.OgrenciNo == existingOgrenci.OgrenciNo &&
                                            od.Ders.DersKodu == existingDers.DersKodu))
                    {
                        _dbContext.OgrenciDersler.Add(new OgrenciDersler
                        {
                            Ogrenci = existingOgrenci,
                            Ders = existingDers
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"OgrenciNo {ogrenciDers.Ogrenci.OgrenciNo}: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync();

            result.Success = result.Errors.Count == 0;
            return result;
        }
    }

    public class SaveResult
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
