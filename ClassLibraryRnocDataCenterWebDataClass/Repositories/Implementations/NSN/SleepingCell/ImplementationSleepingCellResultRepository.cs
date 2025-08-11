using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationSleepingCellResultRepository : ImplementationsRepository<Objtable4gkpireportresult>, InterfaceSleepingCellResultRepository
    {
        public ImplementationSleepingCellResultRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Today result operations
        public async Task<IEnumerable<Objtable4gkpireportresult>> GetTodayResultsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _dbSet
                .Where(x => x.DataDate == today)
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByDateAsync(DateOnly date)
        {
            return await _dbSet
                .Where(x => x.DataDate == date)
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByProvinceAsync(string province, DateOnly? date = null)
        {
            var query = _dbSet.AsQueryable();

            if (date.HasValue)
                query = query.Where(x => x.DataDate == date.Value);

            return await query
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByVendorAsync(string vendor, DateOnly? date = null)
        {
            var query = _dbSet.AsQueryable();

            if (date.HasValue)
                query = query.Where(x => x.DataDate == date.Value);

            return await query
                .Where(x => x.Vendor == vendor)
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();
        }

        // Reset tracking
        public async Task<Objtable4gkpireportresult?> GetResultByCellAsync(string cellName, DateOnly date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.LncelName == cellName && x.DataDate == date);
        }

        public async Task<bool> HasResetResultAsync(int originalId, DateOnly date)
        {
            return await _dbSet
                .AnyAsync(x => x.OriginalId == originalId && x.DataDate == date);
        }

        public async Task<int> GetResetCountByDateAsync(DateOnly date)
        {
            return await _dbSet
                .CountAsync(x => x.DataDate == date);
        }

        public async Task<int> GetResetCountByVendorAsync(string vendor, DateOnly date)
        {
            return await _dbSet
                .CountAsync(x => x.Vendor == vendor && x.DataDate == date);
        }

        // Archive operations
        public async Task<int> ArchiveResultsAsync(DateOnly fromDate, DateOnly toDate)
        {
            var resultsToArchive = await _dbSet
                .Where(x => x.DataDate >= fromDate && x.DataDate <= toDate)
                .ToListAsync();

            foreach (var result in resultsToArchive)
            {
                var archiveRecord = new Objtable4gkpireportresultarchive
                {
                    OriginalId = result.OriginalId,
                    PeriodStartTime = result.PeriodStartTime,
                    MrbtsName = result.MrbtsName,
                    LnbtsName = result.LnbtsName,
                    LncelName = result.LncelName,
                    DnMrbtsSite = result.DnMrbtsSite,
                    PdcpVolumeDl = result.PdcpVolumeDl,
                    PdcpVolumeUl = result.PdcpVolumeUl,
                    CellAvail = result.CellAvail,
                    MaxUes = result.MaxUes,
                    MaxPdcpDl = result.MaxPdcpDl,
                    MaxPdcpUl = result.MaxPdcpUl,
                    OriginalCreatedAt = result.OriginalCreatedAt,
                    DataDate = result.DataDate,
                    Province = result.Province,
                    District = result.District,
                    Region = result.Region,
                    Vendor = result.Vendor,
                    ArchivedAt = DateTime.Now,
                    ArchivedBy = "SYSTEM_AUTO"
                };

                _context.Objtable4gkpireportresultarchives.Add(archiveRecord);
            }

            await SaveChangesAsync();
            return resultsToArchive.Count;
        }

        public async Task<int> ClearTodayResultsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayResults = await _dbSet
                .Where(x => x.DataDate == today)
                .ToListAsync();

            _dbSet.RemoveRange(todayResults);
            await SaveChangesAsync();
            return todayResults.Count;
        }

        // Dashboard data
        public async Task<IEnumerable<Objtable4gkpireportresult>> GetRecentResetResultsAsync(int days = 7)
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
            return await _dbSet
                .Where(x => x.DataDate >= cutoffDate)
                .OrderByDescending(x => x.ArchivedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetResetCountByProvinceAsync(DateOnly date)
        {
            return await _dbSet
                .Where(x => x.DataDate == date && x.Province != null)
                .GroupBy(x => x.Province!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}