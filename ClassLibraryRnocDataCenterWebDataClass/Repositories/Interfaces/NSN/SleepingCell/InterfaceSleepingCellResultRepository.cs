using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;




namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell
{




    public interface InterfaceSleepingCellResultRepository : InterfaceBaseRepository<Objtable4gkpireportresult>
    {
        // Today result operations
        Task<IEnumerable<Objtable4gkpireportresult>> GetTodayResultsAsync();
        Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByDateAsync(DateOnly date);
        Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByProvinceAsync(string province, DateOnly? date = null);
        Task<IEnumerable<Objtable4gkpireportresult>> GetResultsByVendorAsync(string vendor, DateOnly? date = null);

        // Reset tracking
        Task<Objtable4gkpireportresult?> GetResultByCellAsync(string cellName, DateOnly date);
        Task<bool> HasResetResultAsync(int originalId, DateOnly date);
        Task<int> GetResetCountByDateAsync(DateOnly date);
        Task<int> GetResetCountByVendorAsync(string vendor, DateOnly date);

        // Archive operations
        Task<int> ArchiveResultsAsync(DateOnly fromDate, DateOnly toDate);
        Task<int> ClearTodayResultsAsync();

        // Dashboard data
        Task<IEnumerable<Objtable4gkpireportresult>> GetRecentResetResultsAsync(int days = 7);
        Task<Dictionary<string, int>> GetResetCountByProvinceAsync(DateOnly date);
    }
}
