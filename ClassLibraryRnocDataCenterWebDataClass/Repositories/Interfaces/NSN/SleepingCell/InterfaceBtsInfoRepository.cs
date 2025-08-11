using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;



using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceBtsInfoRepository : InterfaceBaseRepository<ObjtablemrbtsInfor>
    {
        // BTS lookup operations
        Task<ObjtablemrbtsInfor?> GetBtsByMrbtsNameAsync(string mrbtsName);
        Task<ObjtablemrbtsInfor?> GetBtsByEnodebNameAsync(string enodebName);
        Task<ObjtablemrbtsInfor?> GetBtsByOamIpAsync(string oamIp);
        Task<IEnumerable<ObjtablemrbtsInfor>> GetBtsByVendorAsync(string vendor);

        // Permission checks
        Task<bool> IsResetAllowedAsync(string mrbtsName);
        Task<bool> IsBlacklistedAsync(string mrbtsName);
        Task<IEnumerable<ObjtablemrbtsInfor>> GetResetEligibleBtsAsync();
        Task<IEnumerable<ObjtablemrbtsInfor>> GetBlacklistedBtsAsync();

        // Blacklist management
        Task SetBlacklistStatusAsync(string mrbtsName, bool isBlacklisted);
        Task SetResetPermissionAsync(string mrbtsName, bool allowReset);
        Task UpdateBtsNotesAsync(string mrbtsName, string notes);

        // Vendor operations
        Task<Dictionary<string, int>> GetBtsCountByVendorAsync();
        Task<IEnumerable<string>> GetAvailableVendorsAsync();
        Task UpdateVendorAsync(string mrbtsName, string vendor);

        // Statistics
        Task<int> GetTotalActiveBtsCountAsync();
        Task<int> GetResetAllowedCountAsync();
        Task<int> GetBlacklistedCountAsync();

        // Bulk operations
        Task<IEnumerable<ObjtablemrbtsInfor>> GetBtsBySitePatternAsync(string pattern);
        Task BulkUpdateVendorAsync(IEnumerable<string> mrbtsNames, string vendor);
    }
}







