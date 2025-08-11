using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationBtsInfoRepository : ImplementationsRepository<ObjtablemrbtsInfor>, InterfaceBtsInfoRepository
    {
        public ImplementationBtsInfoRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // BTS lookup operations
        public async Task<ObjtablemrbtsInfor?> GetBtsByMrbtsNameAsync(string mrbtsName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Mrbtsname == mrbtsName);
        }

        public async Task<ObjtablemrbtsInfor?> GetBtsByEnodebNameAsync(string enodebName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Enodebname == enodebName);
        }

        public async Task<ObjtablemrbtsInfor?> GetBtsByOamIpAsync(string oamIp)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Oam == oamIp);
        }

        public async Task<IEnumerable<ObjtablemrbtsInfor>> GetBtsByVendorAsync(string vendor)
        {
            return await _dbSet
                .Where(x => x.Vendor == vendor)
                .OrderBy(x => x.Mrbtsname)
                .ToListAsync();
        }

        // Permission checks
        public async Task<bool> IsResetAllowedAsync(string mrbtsName)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            return bts?.Reset == true;
        }

        public async Task<bool> IsBlacklistedAsync(string mrbtsName)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            return bts?.Blacklist == true;
        }

        public async Task<IEnumerable<ObjtablemrbtsInfor>> GetResetEligibleBtsAsync()
        {
            return await _dbSet
                .Where(x => x.Reset == true && x.Blacklist != true)
                .OrderBy(x => x.Mrbtsname)
                .ToListAsync();
        }

        public async Task<IEnumerable<ObjtablemrbtsInfor>> GetBlacklistedBtsAsync()
        {
            return await _dbSet
                .Where(x => x.Blacklist == true)
                .OrderBy(x => x.Mrbtsname)
                .ToListAsync();
        }

        // Blacklist management
        public async Task SetBlacklistStatusAsync(string mrbtsName, bool isBlacklisted)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            if (bts != null)
            {
                bts.Blacklist = isBlacklisted;
                await SaveChangesAsync();
            }
        }

        public async Task SetResetPermissionAsync(string mrbtsName, bool allowReset)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            if (bts != null)
            {
                bts.Reset = allowReset;
                await SaveChangesAsync();
            }
        }

        public async Task UpdateBtsNotesAsync(string mrbtsName, string notes)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            if (bts != null)
            {
                bts.Note = notes;
                await SaveChangesAsync();
            }
        }

        // Vendor operations
        public async Task<Dictionary<string, int>> GetBtsCountByVendorAsync()
        {
            return await _dbSet
                .Where(x => x.Vendor != null)
                .GroupBy(x => x.Vendor!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<string>> GetAvailableVendorsAsync()
        {
            return await _dbSet
                .Where(x => x.Vendor != null)
                .Select(x => x.Vendor!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task UpdateVendorAsync(string mrbtsName, string vendor)
        {
            var bts = await GetBtsByMrbtsNameAsync(mrbtsName);
            if (bts != null)
            {
                bts.Vendor = vendor;
                await SaveChangesAsync();
            }
        }

        // Statistics
        public async Task<int> GetTotalActiveBtsCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.Reset == true || x.Blacklist != true);
        }

        public async Task<int> GetResetAllowedCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.Reset == true);
        }

        public async Task<int> GetBlacklistedCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.Blacklist == true);
        }

        // Bulk operations
        public async Task<IEnumerable<ObjtablemrbtsInfor>> GetBtsBySitePatternAsync(string pattern)
        {
            return await _dbSet
                .Where(x => x.Mrbtsname != null && x.Mrbtsname.Contains(pattern))
                .OrderBy(x => x.Mrbtsname)
                .ToListAsync();
        }

        public async Task BulkUpdateVendorAsync(IEnumerable<string> mrbtsNames, string vendor)
        {
            var btsList = await _dbSet
                .Where(x => mrbtsNames.Contains(x.Mrbtsname!))
                .ToListAsync();

            foreach (var bts in btsList)
            {
                bts.Vendor = vendor;
            }


            await SaveChangesAsync();
        }
    }
}