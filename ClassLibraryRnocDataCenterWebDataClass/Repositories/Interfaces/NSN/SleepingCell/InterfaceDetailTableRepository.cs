using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell
{
    public interface InterfaceDetailTableRepository
    {
        Task<Objtable4gkpireportresultdetail> GetOrCreateDetailRecordAsync(string cellName);
        Task UpdateExecutionStatusAsync(long detailId, string status);
        Task UpdateSshConnectionAsync(long detailId, string status, string host);
        Task UpdateResetResultAsync(long detailId, bool success, string output, string executedBy, Dictionary<string, object> updatecellResult);
        Task UpdateExecutionLogAsync(long detailId, string logData);

        Task<Objtable4gkpireportresult> CreateResultFromDetailAsync(long detailId);
        Task<Objtable4gkpireportresultarchive> ArchiveResultRecordAsync(long resultId);

        Task<Objtable4gkpireportresultdetailarchive> ArchiveDetailRecordAsync(long detailId);
        

        }
}