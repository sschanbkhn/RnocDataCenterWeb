using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceSshAccountRepository : InterfaceBaseRepository<Objtableaccountssh>
    {
        // Account lookup operations
        Task<Objtableaccountssh?> GetAccountBySystemAsync(string system);
        Task<IEnumerable<Objtableaccountssh>> GetActiveAccountsAsync();
        Task<IEnumerable<Objtableaccountssh>> GetAccountsBySystemPatternAsync(string systemPattern);

        // Connection validation
        Task<bool> IsAccountActiveAsync(string system);
        Task<bool> ValidateCredentialsAsync(string system);
        Task<Objtableaccountssh?> GetBestAccountForSystemAsync(string system);

        // Account management
        Task SetAccountStatusAsync(string system, bool isActive);
        Task UpdateCredentialsAsync(string system, string username, string password);
        Task UpdatePortAsync(string system, int port);

        // System operations
        Task<IEnumerable<string>> GetAvailableSystemsAsync();
        Task<Dictionary<string, bool>> GetSystemStatusAsync();
        Task<int> GetActiveAccountCountAsync();

        // Security operations
        Task<DateTime?> GetLastUsedAsync(string system);
        Task UpdateLastUsedAsync(string system);
        Task<IEnumerable<Objtableaccountssh>> GetUnusedAccountsAsync(int days = 30);

        // Bulk operations
        Task SetMultipleAccountStatusAsync(IEnumerable<string> systems, bool isActive);
        Task<bool> HasCredentialsForVendorAsync(string vendor);
    }
}

