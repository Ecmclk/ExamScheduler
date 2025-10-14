using ExamScheduler.Core.Interfaces;
using ExamScheduler.Data;
using ExamScheduler.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamScheduler.Services
{
    public class DersService
    {
        private readonly AppDbContext _dbContext;

        public DersService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaveResultDers> SaveDerslerAsync(ParseResult<Dersler> parseResult)
        {
            var result = new SaveResultDers();

            foreach (var ders in parseResult.Data)
            {
                try
                {
                    // Mevcut ders kontrolü
                    var existingDers = await _dbContext.Dersler
                        .FirstOrDefaultAsync(d => d.DersKodu == ders.DersKodu);

                    if (existingDers == null)
                    {
                        _dbContext.Dersler.Add(ders);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Ders {ders.DersKodu}: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync();
            result.Success = result.Errors.Count == 0;
            return result;
        }
        
        public async Task<List<Dersler>> GetAllDerslerAsync()
        {
            return await _dbContext.Dersler
                .Include(d => d.OgrenciDersler)
                .ThenInclude(od => od.Ogrenci)
                .ToListAsync();
        }
        
        public async Task<List<Ogrenciler>> GetOgrencilerByDersIdAsync(int dersId)
        {
            var ders = await _dbContext.Dersler
                .Include(d => d.OgrenciDersler)
                .ThenInclude(od => od.Ogrenci)
                .FirstOrDefaultAsync(d => d.DersId == dersId);

            return ders?.OgrenciDersler.Select(od => od.Ogrenci).ToList() ?? new List<Ogrenciler>();
        }
        
        public async Task<List<object>> GetFlatDersListAsync()
        {
            var dersler = await _dbContext.Dersler
                .Include(d => d.OgrenciDersler)
                .ThenInclude(od => od.Ogrenci)
                .ToListAsync();

            var flatList = dersler.Select(d => new
            {
                DersId = d.DersId,      // <-- burayı ekle
                DersKodu = d.DersKodu,
                DersAdi = d.DersAdi,
                OgrenciSayisi = d.OgrenciDersler.Count
            }).ToList<object>();

            return flatList;
        }

    }

 
    public class SaveResultDers
    {
        public bool Success { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
}