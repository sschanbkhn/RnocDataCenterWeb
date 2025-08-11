using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationSshAccountRepository : ImplementationsRepository<Objtableaccountssh>, InterfaceSshAccountRepository
    {
        public ImplementationSshAccountRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // Account lookup operations
        public async Task<Objtableaccountssh?> GetAccountBySystemAsync(string system)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.System == system);
        }

        public async Task<IEnumerable<Objtableaccountssh>> GetActiveAccountsAsync()
        {
            return await _dbSet
                .Where(x => x.Active == true)
                .OrderBy(x => x.System)
                .ToListAsync();
        }

        public async Task<IEnumerable<Objtableaccountssh>> GetAccountsBySystemPatternAsync(string systemPattern)
        {
            return await _dbSet
                .Where(x => x.System != null && x.System.Contains(systemPattern))
                .OrderBy(x => x.System)
                .ToListAsync();
        }

        // Connection validation
        public async Task<bool> IsAccountActiveAsync(string system)
        {
            var account = await GetAccountBySystemAsync(system);
            return account?.Active == true;
        }

        public async Task<bool> ValidateCredentialsAsync(string system)
        {
            var account = await GetAccountBySystemAsync(system);
            return account?.Active == true &&
                   !string.IsNullOrEmpty(account.Usename) &&
                   !string.IsNullOrEmpty(account.Password);
        }

        public async Task<Objtableaccountssh?> GetBestAccountForSystemAsync(string system)
        {
            // Try exact match first
            var exactAccount = await _dbSet
                .FirstOrDefaultAsync(x => x.System == system && x.Active == true);

            if (exactAccount != null)
                return exactAccount;

            // Try pattern match as fallback
            var patternAccount = await _dbSet
                .Where(x => x.Active == true && x.System != null && system.Contains(x.System))
                .OrderByDescending(x => x.System!.Length)
                .FirstOrDefaultAsync();

            return patternAccount;
        }

        // Account management
        public async Task SetAccountStatusAsync(string system, bool isActive)
        {
            var account = await GetAccountBySystemAsync(system);
            if (account != null)
            {
                account.Active = isActive;
                await SaveChangesAsync();
            }
        }

        public async Task UpdateCredentialsAsync(string system, string username, string password)
        {
            var account = await GetAccountBySystemAsync(system);
            if (account != null)
            {
                account.Usename = username;
                account.Password = password;
                await SaveChangesAsync();
            }
        }

        public async Task UpdatePortAsync(string system, int port)
        {
            var account = await GetAccountBySystemAsync(system);
            if (account != null)
            {
                account.Port = port;
                await SaveChangesAsync();
            }
        }

        // System operations
        public async Task<IEnumerable<string>> GetAvailableSystemsAsync()
        {
            return await _dbSet
                .Where(x => x.System != null && x.Active == true)
                .Select(x => x.System!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<Dictionary<string, bool>> GetSystemStatusAsync()
        {
            return await _dbSet
                .Where(x => x.System != null)
                .ToDictionaryAsync(x => x.System!, x => x.Active ?? false);
        }

        public async Task<int> GetActiveAccountCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.Active == true);
        }

        // Security operations  
        public async Task<DateTime?> GetLastUsedAsync(string system)
        {
            var account = await GetAccountBySystemAsync(system);
            // Note: Add LastUsedTime field to entity if needed for tracking
            return null; // Placeholder - implement based on tracking requirements
        }

        public async Task UpdateLastUsedAsync(string system)
        {
            var account = await GetAccountBySystemAsync(system);
            if (account != null)
            {
                // Note: Add LastUsedTime field to entity if needed
                // account.LastUsedTime = DateTime.Now;
                await SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Objtableaccountssh>> GetUnusedAccountsAsync(int days = 30)
        {
            // Note: Implement based on LastUsedTime field when available
            // For now, return accounts that are inactive
            return await _dbSet
                .Where(x => x.Active != true)
                .OrderBy(x => x.System)
                .ToListAsync();
        }

        // Bulk operations
        public async Task SetMultipleAccountStatusAsync(IEnumerable<string> systems, bool isActive)
        {
            var accounts = await _dbSet
                .Where(x => systems.Contains(x.System!))
                .ToListAsync();

            foreach (var account in accounts)
            {
                account.Active = isActive;
            }

            await SaveChangesAsync();
        }

        public async Task<bool> HasCredentialsForVendorAsync(string vendor)
        {
            return await _dbSet
                .AnyAsync(x => x.Active == true &&
                              x.System != null &&
                              x.System.ToUpper().Contains(vendor.ToUpper()));
        }
    }
}