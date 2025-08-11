using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationResetLimitRepository : ImplementationsRepository<Objtableresetsitecountlimit>, InterfaceResetLimitRepository
    {
        public ImplementationResetLimitRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Daily limit operations
        public async Task<Objtableresetsitecountlimit?> GetTodayLimitsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _dbSet
                .FirstOrDefaultAsync(x => x.LimitDate == today);
        }

        public async Task<Objtableresetsitecountlimit?> GetLimitsByDateAsync(DateOnly date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.LimitDate == date);
        }

        public async Task<bool> CanResetMoreSitesAsync(int requestedCount = 1)
        {
            var todayLimit = await GetTodayLimitsAsync();
            if (todayLimit == null) return false;

            var currentUsage = todayLimit.SitesResetToday ?? 0;
            /// var maxLimit = todayLimit.MaxSitesPerDay ?? 50;
            /// SỬA THÀNH:
            var maxLimit = todayLimit.MaxSitesPerDay; // Vì đã là int, không cần ??

            return (currentUsage + requestedCount) <= maxLimit;
        }

        public async Task<bool> CanResetVendorSitesAsync(string vendor, int requestedCount = 1)
        {
            var todayLimit = await GetTodayLimitsAsync();
            if (todayLimit == null) return false;

            return vendor.ToUpper() switch
            {
                "NSN" => (todayLimit.NsnSitesReset ?? 0) + requestedCount <= (todayLimit.MaxNsnSites ?? 20),
                "ERICSSON" => (todayLimit.EricssonSitesReset ?? 0) + requestedCount <= (todayLimit.MaxEricssonSites ?? 15),
                "HUAWEI" => (todayLimit.HuaweiSitesReset ?? 0) + requestedCount <= (todayLimit.MaxHuaweiSites ?? 15),
                _ => false
            };
        }

        // Counter updates
        public async Task UpdateResetCountAsync(int additionalSites)
        {
            var todayLimit = await GetOrCreateTodayLimitAsync();
            todayLimit.SitesResetToday = (todayLimit.SitesResetToday ?? 0) + additionalSites;
            todayLimit.LastResetTime = DateTime.Now;
            todayLimit.UpdatedAt = DateTime.Now;

            await SaveChangesAsync();
        }

        public async Task UpdateVendorResetCountAsync(string vendor, int additionalSites)
        {
            var todayLimit = await GetOrCreateTodayLimitAsync();

            switch (vendor.ToUpper())
            {
                case "NSN":
                    todayLimit.NsnSitesReset = (todayLimit.NsnSitesReset ?? 0) + additionalSites;
                    break;
                case "ERICSSON":
                    todayLimit.EricssonSitesReset = (todayLimit.EricssonSitesReset ?? 0) + additionalSites;
                    break;
                case "HUAWEI":
                    todayLimit.HuaweiSitesReset = (todayLimit.HuaweiSitesReset ?? 0) + additionalSites;
                    break;
            }

            todayLimit.UpdatedAt = DateTime.Now;
            await SaveChangesAsync();
        }

        public async Task ResetDailyCountersAsync(DateOnly date)
        {
            var limitRecord = await GetLimitsByDateAsync(date);
            if (limitRecord != null)
            {
                limitRecord.SitesResetToday = 0;
                limitRecord.NsnSitesReset = 0;
                limitRecord.EricssonSitesReset = 0;
                limitRecord.HuaweiSitesReset = 0;
                limitRecord.UpdatedAt = DateTime.Now;

                await SaveChangesAsync();
            }
        }

        // Limit management
        public async Task<bool> IsAutoResetEnabledAsync()
        {
            var todayLimit = await GetTodayLimitsAsync();
            return todayLimit?.AutoResetEnabled ?? true;
        }

        public async Task SetAutoResetEnabledAsync(bool enabled)
        {
            var todayLimit = await GetOrCreateTodayLimitAsync();
            todayLimit.AutoResetEnabled = enabled;
            todayLimit.UpdatedAt = DateTime.Now;

            await SaveChangesAsync();
        }

        public async Task<int> GetRemainingResetQuotaAsync()
        {
            var todayLimit = await GetTodayLimitsAsync();
            if (todayLimit == null) return 0;

            /// var maxLimit = todayLimit.MaxSitesPerDay ?? 50;

            var maxLimit = todayLimit.MaxSitesPerDay;
            var currentUsage = todayLimit.SitesResetToday ?? 0;

            return Math.Max(0, maxLimit - currentUsage);
        }

        public async Task<int> GetRemainingVendorQuotaAsync(string vendor)
        {
            var todayLimit = await GetTodayLimitsAsync();
            if (todayLimit == null) return 0;

            return vendor.ToUpper() switch
            {
                "NSN" => Math.Max(0, (todayLimit.MaxNsnSites ?? 20) - (todayLimit.NsnSitesReset ?? 0)),
                "ERICSSON" => Math.Max(0, (todayLimit.MaxEricssonSites ?? 15) - (todayLimit.EricssonSitesReset ?? 0)),
                "HUAWEI" => Math.Max(0, (todayLimit.MaxHuaweiSites ?? 15) - (todayLimit.HuaweiSitesReset ?? 0)),
                _ => 0
            };
        }

        // Configuration
        public async Task SetDailyLimitAsync(int maxSites)
        {
            var todayLimit = await GetOrCreateTodayLimitAsync();
            todayLimit.MaxSitesPerDay = maxSites;
            todayLimit.UpdatedAt = DateTime.Now;

            await SaveChangesAsync();
        }

        public async Task SetVendorLimitAsync(string vendor, int maxSites)
        {
            var todayLimit = await GetOrCreateTodayLimitAsync();

            switch (vendor.ToUpper())
            {
                case "NSN":
                    todayLimit.MaxNsnSites = maxSites;
                    break;
                case "ERICSSON":
                    todayLimit.MaxEricssonSites = maxSites;
                    break;
                case "HUAWEI":
                    todayLimit.MaxHuaweiSites = maxSites;
                    break;
            }

            todayLimit.UpdatedAt = DateTime.Now;
            await SaveChangesAsync();
        }

        public async Task<Dictionary<string, int>> GetVendorLimitsAsync()
        {
            var todayLimit = await GetTodayLimitsAsync();
            if (todayLimit == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                ["NSN"] = todayLimit.MaxNsnSites ?? 20,
                ["ERICSSON"] = todayLimit.MaxEricssonSites ?? 15,
                ["HUAWEI"] = todayLimit.MaxHuaweiSites ?? 15
            };
        }

        public async Task<Dictionary<string, int>> GetVendorUsageAsync(DateOnly date)
        {
            var limitRecord = await GetLimitsByDateAsync(date);
            if (limitRecord == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                ["NSN"] = limitRecord.NsnSitesReset ?? 0,
                ["ERICSSON"] = limitRecord.EricssonSitesReset ?? 0,
                ["HUAWEI"] = limitRecord.HuaweiSitesReset ?? 0
            };
        }

        // Historical limits
        public async Task<IEnumerable<Objtableresetsitecountlimit>> GetLimitHistoryAsync(DateOnly fromDate, DateOnly toDate)
        {
            return await _dbSet
                .Where(x => x.LimitDate >= fromDate && x.LimitDate <= toDate)
                .OrderByDescending(x => x.LimitDate)
                .ToListAsync();
        }

        public async Task<int> GetAverageUsageAsync(int days = 30)
        {
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
            var records = await _dbSet
                .Where(x => x.LimitDate >= fromDate)
                .ToListAsync();

            if (!records.Any()) return 0;

            return (int)records.Average(x => x.SitesResetToday ?? 0);
        }

        // Helper method
        private async Task<Objtableresetsitecountlimit> GetOrCreateTodayLimitAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayLimit = await GetLimitsByDateAsync(today);

            if (todayLimit == null)
            {
                todayLimit = new Objtableresetsitecountlimit
                {
                    LimitDate = today,
                    MaxSitesPerDay = 50,
                    MaxNsnSites = 20,
                    MaxEricssonSites = 15,
                    MaxHuaweiSites = 15,
                    SitesResetToday = 0,
                    NsnSitesReset = 0,
                    EricssonSitesReset = 0,
                    HuaweiSitesReset = 0,
                    AutoResetEnabled = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _dbSet.AddAsync(todayLimit);
                await SaveChangesAsync();
            }

            return todayLimit;
        }
    }

}