using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceFilePathRepository : InterfaceBaseRepository<Tablefilepath>
    {
        // File path configuration
        Task<Tablefilepath?> GetFilePathByOssAsync(string oss);
        Task<IEnumerable<Tablefilepath>> GetActiveFilePathsAsync();
        Task<IEnumerable<Tablefilepath>> GetFilePathsByProtocolAsync(string protocol);

        // Connection management
        Task<bool> IsFilePathActiveAsync(string oss);
        Task SetFilePathStatusAsync(string oss, bool isActive);
        Task UpdateConnectionInfoAsync(string oss, string host, int port, string username, string password);

        // File operations  
        Task<string?> GetFilePathForOssAsync(string oss);
        Task<bool> ValidateConnectionAsync(string oss);
        Task UpdateLastAccessTimeAsync(string oss);

        // Statistics
        Task<int> GetActiveFilePathCountAsync();
        Task<Dictionary<string, string>> GetOssFilePathMappingAsync();
    }
}
