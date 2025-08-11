using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using System.Linq.Expressions;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;


namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationSleepingCellKpiRepository : ImplementationsRepository<Objtable4gkpireport>, InterfaceSleepingCellKpiRepository
    {
        public ImplementationSleepingCellKpiRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Raw KPI data operations
        public async Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsAsync()
        {
            return await _dbSet
                .Where(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByProvinceAsync(string province)
        {
            return await _dbSet
                .Where(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0)
                .Where(x => x.MrbtsName != null && x.MrbtsName.Contains(province))
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByVendorAsync(string vendor)
        {
            return await _dbSet
                .Where(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0)
                .Where(x => x.MrbtsName != null && x.MrbtsName.Contains(vendor.ToUpper()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetCellsWithLowTrafficAsync(decimal threshold = 1.0m)
        {
            return await _dbSet
                .Where(x => (x.PdcpVolumeDl ?? 0) + (x.PdcpVolumeUl ?? 0) <= threshold)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0) // ← THÊM ĐIỀU KIỆN SLEEPING
                .Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetKpiDataByMrbtsAsync(string mrbtsName)
        {
            return await _dbSet
                .Where(x => x.MrbtsName == mrbtsName)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetSleepingCellCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0);
        }

        public async Task<int> GetSleepingCellCountByProvinceAsync(string province)
        {
            return await _dbSet
                .CountAsync(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0
                    && x.MrbtsName != null && x.MrbtsName.Contains(province));
        }

        public async Task<bool> HasSleepingCellsAsync()
        {
            return await _dbSet
                .AnyAsync(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0);
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetLatestKpiDataAsync(int hours = 24)
        {
            var cutoffTime = DateTime.Now.AddHours(-hours);
            return await _dbSet
                .Where(x => x.CreatedAt >= cutoffTime)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetSleepingCellStatsByProvinceAsync()
        {
            var sleepingCells = await _dbSet
                .Where(x => x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0 && x.MrbtsName != null)
                .ToListAsync();

            return sleepingCells
                .GroupBy(x => ExtractProvinceFromMrbts(x.MrbtsName!))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Objtable4gkpireport>> GetCellsRequiringAttentionAsync()
        {
            return await _dbSet
                .Where(x => (x.PdcpVolumeDl == 0 && x.PdcpVolumeUl == 0) ||
                           (x.CellAvail ?? 0) < 95)
                .ToListAsync();
        }

        // Helper method
        private string ExtractProvinceFromMrbts(string mrbtsName)
        {
            if (mrbtsName.Contains("HN")) return "Hà Nội";
            if (mrbtsName.Contains("HCM")) return "TP.HCM";
            if (mrbtsName.Contains("DN")) return "Đà Nẵng";
            return "Khác";
        }
    }
}