using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationDashboardRepository : ImplementationsRepository<VKpiResultToday>, InterfaceDashboardRepository
    {
        public ImplementationDashboardRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Today's dashboard data
        public async Task<IEnumerable<VKpiResultToday>> GetTodayKpiSummaryAsync()
        {
            return await _dbSet
                .OrderByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        public async Task<IEnumerable<VKpiResultToday>> GetTodayKpiByProvinceAsync(string province)
        {
            return await _dbSet
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        public async Task<IEnumerable<VKpiResultToday>> GetTodayKpiByVendorAsync(string vendor)
        {
            return await _dbSet
                .Where(x => x.Vendor == vendor)
                .OrderByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        public async Task<IEnumerable<VKpiResultToday>> GetTodayKpiByRegionAsync(string region)
        {
            return await _dbSet
                .Where(x => x.Region == region)
                .OrderByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        // Archive dashboard data
        public async Task<IEnumerable<VKpiArchiveSummary>> GetArchiveKpiSummaryAsync(DateOnly fromDate, DateOnly toDate)
        {
            return await _context.VKpiArchiveSummaries
                .Where(x => x.DataDate >= fromDate && x.DataDate <= toDate)
                .OrderByDescending(x => x.DataDate)
                .ThenByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        public async Task<IEnumerable<VKpiArchiveSummary>> GetMonthlyKpiSummaryAsync(int year, int month)
        {
            return await _context.VKpiArchiveSummaries
                .Where(x => x.DataYear == year && x.DataMonth == month)
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VKpiArchiveSummary>> GetQuarterlyKpiSummaryAsync(int year, int quarter)
        {
            return await _context.VKpiArchiveSummaries
                .Where(x => x.DataYear == year && x.DataQuarter == quarter)
                .OrderByDescending(x => x.DataDate)
                .ToListAsync();
        }

        // Statistics aggregation - FIX CONVERSION
        public async Task<int> GetTotalSleepingCellsTodayAsync()
        {
            var result = await _dbSet.SumAsync(x => x.SleepingCells ?? 0);
            return (int)result;
        }

        public async Task<int> GetTotalCellsTodayAsync()
        {
            var result = await _dbSet.SumAsync(x => x.TotalCells ?? 0);
            return (int)result;
        }

        public async Task<decimal> GetSleepingCellRateTodayAsync()
        {
            var totalCells = await GetTotalCellsTodayAsync();
            var sleepingCells = await GetTotalSleepingCellsTodayAsync();

            if (totalCells == 0) return 0;

            return Math.Round((decimal)sleepingCells / totalCells * 100, 2);
        }

        public async Task<Dictionary<string, int>> GetSleepingCellsByProvinceAsync()
        {
            return await _dbSet
                .Where(x => x.Province != null)
                .GroupBy(x => x.Province!)
                .ToDictionaryAsync(g => g.Key, g => (int)g.Sum(x => x.SleepingCells ?? 0));
        }

        public async Task<Dictionary<string, int>> GetSleepingCellsByVendorAsync()
        {
            return await _dbSet
                .Where(x => x.Vendor != null)
                .GroupBy(x => x.Vendor!)
                .ToDictionaryAsync(g => g.Key, g => (int)g.Sum(x => x.SleepingCells ?? 0));
        }

        // Trend analysis
        public async Task<IEnumerable<VKpiArchiveSummary>> GetSleepingCellTrendAsync(int days = 30)
        {
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));

            return await _context.VKpiArchiveSummaries
                .Where(x => x.DataDate >= fromDate)
                .OrderBy(x => x.DataDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetProvincePerformanceTrendAsync(string province, int days = 7)
        {
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));

            var trends = await _context.VKpiArchiveSummaries
                .Where(x => x.Province == province && x.DataDate >= fromDate)
                .GroupBy(x => x.DataDate)
                .Select(g => new {
                    Date = g.Key,
                    Rate = g.Sum(x => x.SleepingCells ?? 0) * 100.0m / Math.Max(1, g.Sum(x => x.TotalCells ?? 0))
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return trends.ToDictionary(
                x => x.Date.ToString(),
                x => Math.Round(x.Rate, 2)
            );
        }

        public async Task<Dictionary<string, decimal>> GetVendorPerformanceTrendAsync(string vendor, int days = 7)
        {
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));

            var trends = await _context.VKpiArchiveSummaries
                .Where(x => x.Vendor == vendor && x.DataDate >= fromDate)
                .GroupBy(x => x.DataDate)
                .Select(g => new {
                    Date = g.Key,
                    Rate = g.Sum(x => x.SleepingCells ?? 0) * 100.0m / Math.Max(1, g.Sum(x => x.TotalCells ?? 0))
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return trends.ToDictionary(
                x => x.Date.ToString(),
                x => Math.Round(x.Rate, 2)
            );
        }

        // Alert data - FIX DECIMAL COMPARISON
        public async Task<IEnumerable<VKpiResultToday>> GetProblemsRequiringAttentionAsync()
        {
            var highSleepingCellThreshold = 100;
            var lowAvailabilityThreshold = 95.0m;

            return await _dbSet
                .Where(x => (x.SleepingCells ?? 0) > highSleepingCellThreshold ||
                           (x.AvgAvailability ?? 0) < lowAvailabilityThreshold)
                .OrderByDescending(x => x.SleepingCells)
                .ToListAsync();
        }

        public async Task<bool> HasCriticalIssuesAsync()
        {
            var criticalThreshold = 200;

            return await _dbSet
                .AnyAsync(x => (x.SleepingCells ?? 0) > criticalThreshold);
        }

        public async Task<int> GetHighPriorityAlertCountAsync()
        {
            var highPriorityThreshold = 50;

            return await _dbSet
                .CountAsync(x => (x.SleepingCells ?? 0) > highPriorityThreshold);
        }

        // Performance metrics
        public async Task<decimal> GetOverallNetworkHealthScoreAsync()
        {
            var totalCells = await GetTotalCellsTodayAsync();
            var sleepingCells = await GetTotalSleepingCellsTodayAsync();

            if (totalCells == 0) return 100;

            var healthScore = 100 - ((decimal)sleepingCells / totalCells * 100);
            return Math.Round(Math.Max(0, healthScore), 2);
        }

        public async Task<Dictionary<string, decimal>> GetRegionalHealthScoresAsync()
        {
            var regionalData = await _dbSet
                .Where(x => x.Region != null)
                .GroupBy(x => x.Region!)
                .Select(g => new {
                    Region = g.Key,
                    TotalCells = g.Sum(x => x.TotalCells ?? 0),
                    SleepingCells = g.Sum(x => x.SleepingCells ?? 0)
                })
                .ToListAsync();

            return regionalData.ToDictionary(
                x => x.Region,
                x => {
                    if (x.TotalCells == 0) return 100m;
                    var healthScore = 100m - ((decimal)x.SleepingCells / x.TotalCells * 100);
                    return Math.Round(Math.Max(0, healthScore), 2);
                }
            );
        }
    }
}