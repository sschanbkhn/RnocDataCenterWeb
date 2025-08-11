using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationFilePathRepository : ImplementationsRepository<Tablefilepath>, InterfaceFilePathRepository
    {
        public ImplementationFilePathRepository(ConnectionsInformationSleepingCellDbContext context) : base(context)
        {
        }

        // File path configuration
        public async Task<Tablefilepath?> GetFilePathByOssAsync(string oss)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Oss == oss);
        }

        public async Task<IEnumerable<Tablefilepath>> GetActiveFilePathsAsync()
        {
            return await _dbSet
                .Where(x => x.Active == true)
                .OrderBy(x => x.Oss)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tablefilepath>> GetFilePathsByProtocolAsync(string protocol)
        {
            return await _dbSet
                .Where(x => x.Protocol == protocol && x.Active == true)
                .OrderBy(x => x.Oss)
                .ToListAsync();
        }

        // Connection management
        public async Task<bool> IsFilePathActiveAsync(string oss)
        {
            var filePath = await GetFilePathByOssAsync(oss);
            return filePath?.Active == true;
        }

        public async Task SetFilePathStatusAsync(string oss, bool isActive)
        {
            var filePath = await GetFilePathByOssAsync(oss);
            if (filePath != null)
            {
                filePath.Active = isActive;
                await SaveChangesAsync();
            }
        }

        public async Task UpdateConnectionInfoAsync(string oss, string host, int port, string username, string password)
        {
            var filePath = await GetFilePathByOssAsync(oss);
            if (filePath != null)
            {
                filePath.Host = host;
                filePath.Port = port;
                filePath.Username = username;
                filePath.Password = password;
                await SaveChangesAsync();
            }
        }

        // File operations
        public async Task<string?> GetFilePathForOssAsync(string oss)
        {
            var filePathRecord = await GetFilePathByOssAsync(oss);
            return filePathRecord?.Filepath;
        }

        public async Task<bool> ValidateConnectionAsync(string oss)
        {
            var filePath = await GetFilePathByOssAsync(oss);

            if (filePath == null || filePath.Active != true)
                return false;

            // Basic validation - check required fields
            return !string.IsNullOrEmpty(filePath.Host) &&
                   !string.IsNullOrEmpty(filePath.Username) &&
                   !string.IsNullOrEmpty(filePath.Password) &&
                   !string.IsNullOrEmpty(filePath.Filepath) &&
                   filePath.Port > 0;
        }

        public async Task UpdateLastAccessTimeAsync(string oss)
        {
            var filePath = await GetFilePathByOssAsync(oss);
            if (filePath != null)
            {
                // Note: Add LastAccessTime field to entity if needed for tracking
                // filePath.LastAccessTime = DateTime.Now;
                await SaveChangesAsync();
            }
        }

        // Statistics
        public async Task<int> GetActiveFilePathCountAsync()
        {
            return await _dbSet
                .CountAsync(x => x.Active == true);
        }

        public async Task<Dictionary<string, string>> GetOssFilePathMappingAsync()
        {
            return await _dbSet
                .Where(x => x.Active == true && x.Oss != null && x.Filepath != null)
                .ToDictionaryAsync(x => x.Oss!, x => x.Filepath!);
        }
    }
}