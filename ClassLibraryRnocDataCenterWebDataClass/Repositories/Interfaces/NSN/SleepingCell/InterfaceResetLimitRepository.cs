using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceResetLimitRepository : InterfaceBaseRepository<Objtableresetsitecountlimit>
    {
        // Daily limit operations
        Task<Objtableresetsitecountlimit?> GetTodayLimitsAsync();
        Task<Objtableresetsitecountlimit?> GetLimitsByDateAsync(DateOnly date);
        Task<bool> CanResetMoreSitesAsync(int requestedCount = 1);
        Task<bool> CanResetVendorSitesAsync(string vendor, int requestedCount = 1);

        // Counter updates
        Task UpdateResetCountAsync(int additionalSites);
        Task UpdateVendorResetCountAsync(string vendor, int additionalSites);
        Task ResetDailyCountersAsync(DateOnly date);

        // Limit management
        Task<bool> IsAutoResetEnabledAsync();
        Task SetAutoResetEnabledAsync(bool enabled);
        Task<int> GetRemainingResetQuotaAsync();
        Task<int> GetRemainingVendorQuotaAsync(string vendor);

        // Configuration
        Task SetDailyLimitAsync(int maxSites);
        Task SetVendorLimitAsync(string vendor, int maxSites);
        Task<Dictionary<string, int>> GetVendorLimitsAsync();
        Task<Dictionary<string, int>> GetVendorUsageAsync(DateOnly date);

        // Historical limits
        Task<IEnumerable<Objtableresetsitecountlimit>> GetLimitHistoryAsync(DateOnly fromDate, DateOnly toDate);
        Task<int> GetAverageUsageAsync(int days = 30);
    }
}