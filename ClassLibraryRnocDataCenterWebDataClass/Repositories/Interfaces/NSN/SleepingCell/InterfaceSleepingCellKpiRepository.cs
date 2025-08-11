using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;


namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell
{
    public interface InterfaceSleepingCellKpiRepository : InterfaceBaseRepository<Objtable4gkpireport>
    {
        // Basic sleeping cell queries
        Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsAsync();
        Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByProvinceAsync(string province);
        Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByDateAsync(DateTime date);
        Task<int> GetSleepingCellCountAsync();

        // Extended sleeping cell operations
        Task<IEnumerable<Objtable4gkpireport>> GetSleepingCellsByVendorAsync(string vendor);
        Task<IEnumerable<Objtable4gkpireport>> GetCellsWithLowTrafficAsync(decimal threshold = 1.0m);
        Task<IEnumerable<Objtable4gkpireport>> GetKpiDataByMrbtsAsync(string mrbtsName);
        Task<int> GetSleepingCellCountByProvinceAsync(string province);
        Task<bool> HasSleepingCellsAsync();
        Task<Dictionary<string, int>> GetSleepingCellStatsByProvinceAsync();

        // Additional useful methods
        Task<IEnumerable<Objtable4gkpireport>> GetLatestKpiDataAsync(int hours = 24);
        Task<IEnumerable<Objtable4gkpireport>> GetCellsRequiringAttentionAsync();

    }
}

