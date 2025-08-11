using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell
{
    public interface InterfaceFilterTableRepository
    {
        Task<IEnumerable<Objtablefilterltekpireport>> GetAllSleepingCellsAsync();
        Task<int> GetSleepingCellCountAsync();
        Task<Objtablefilterltekpireport> GetSleepingCellByNameAsync(string cellName);
        Task<bool> HasSleepingCellsAsync();
    }
}