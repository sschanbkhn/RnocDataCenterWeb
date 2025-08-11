using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationSleepingCellArchiveRepository : ImplementationsRepository<Objtable4gkpireportresultarchive>, InterfaceSleepingCellArchiveRepository
    {
        public ImplementationSleepingCellArchiveRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Historical data queries
        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByDateRangeAsync(DateOnly fromDate, DateOnly toDate)
        {
            return await _dbSet
                .Where(x => x.DataDate >= fromDate && x.DataDate <= toDate)
                .OrderByDescending(x => x.DataDate)
                .ThenByDescending(x => x.ArchivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByMonthAsync(int year, int month)
        {
            return await _dbSet
                .Where(x => x.DataYear == year && x.DataMonth == month)
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByQuarterAsync(int year, int quarter)
        {
            return await _dbSet
                .Where(x => x.DataYear == year && x.DataQuarter == quarter)
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByYearAsync(int year)
        {
            return await _dbSet
                .Where(x => x.DataYear == year)
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        // Province/Region queries
        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByProvinceAsync(string province, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _dbSet.Where(x => x.Province == province);

            if (fromDate.HasValue)
                query = query.Where(x => x.DataDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DataDate <= toDate.Value);

            return await query
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByRegionAsync(string region, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _dbSet.Where(x => x.Region == region);

            if (fromDate.HasValue)
                query = query.Where(x => x.DataDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DataDate <= toDate.Value);

            return await query
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        // Statistics
        public async Task<int> GetTotalArchiveRecordsAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<DateTime> GetOldestArchiveDateAsync()
        {
            var oldestRecord = await _dbSet
                .OrderBy(x => x.DataDate)
                .FirstOrDefaultAsync();

            return oldestRecord?.DataDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        }

        public async Task<DateTime> GetLatestArchiveDateAsync()
        {
            var latestRecord = await _dbSet
                .OrderByDescending(x => x.DataDate)
                .FirstOrDefaultAsync();

            return latestRecord?.DataDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        }

        // Cleanup operations
        public async Task<int> DeleteOldArchiveAsync(DateOnly beforeDate)
        {
            var oldRecords = await _dbSet
                .Where(x => x.DataDate < beforeDate)
                .ToListAsync();

            if (oldRecords.Any())
            {
                _dbSet.RemoveRange(oldRecords);
                await SaveChangesAsync();
            }

            return oldRecords.Count;
        }
    }
}