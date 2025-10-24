

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
        
        public async Task<List<Ogrenciler>> GetAllOgrencilerAsync()
        {
            return await _dbContext.Ogrenciler
                .Include(o => o.Bolum) // Bölüm bilgisi
                .Include(o => o.OgrenciDersler)
                .ThenInclude(od => od.Ders)
                .ToListAsync();
        }
        
        public async Task<Ogrenciler?> GetOgrenciWithDerslerAsync(string ogrNo)
        {
            return await _dbContext.Ogrenciler
                .Where(o => o.OgrenciNo == ogrNo)
                .Include(o => o.OgrenciDersler)
                .ThenInclude(od => od.Ders)
                .FirstOrDefaultAsync();
        }
        
        public async Task<List<object>> GetFlatOgrenciDersListAsync()
        {
            var ogrenciler = await _dbContext.Ogrenciler
                .Include(o => o.OgrenciDersler)
                .ThenInclude(od => od.Ders)
                .ToListAsync();

            var flatList = ogrenciler.SelectMany(o => o.OgrenciDersler.Select(od => new
            {
                OgrenciNo = o.OgrenciNo,
                AdSoyad = o.AdSoyad,
                DersKodu = od.Ders.DersKodu,
                DersAdi = od.Ders.DersAdi
            })).ToList<object>();

            return flatList;
        }
        
        public async Task<List<OgrenciDersler>> GetAllOgrenciDerslerAsync()
        {
            // Öğrenci - Ders ilişkilerini getiriyoruz
            return await _dbContext.OgrenciDersler
                .Include(od => od.Ogrenci)
                .Include(od => od.Ders)
                .ToListAsync();
        }
    }

    public class SaveResult
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
