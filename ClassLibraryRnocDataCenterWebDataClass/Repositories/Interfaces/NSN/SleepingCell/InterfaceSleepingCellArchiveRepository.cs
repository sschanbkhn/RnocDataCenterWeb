using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceSleepingCellArchiveRepository : InterfaceBaseRepository<Objtable4gkpireportresultarchive>
    {
        // Historical data queries
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByDateRangeAsync(DateOnly fromDate, DateOnly toDate);
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByMonthAsync(int year, int month);
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByQuarterAsync(int year, int quarter);
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByYearAsync(int year);

        // Province/Region queries
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByProvinceAsync(string province, DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<IEnumerable<Objtable4gkpireportresultarchive>> GetArchiveByRegionAsync(string region, DateOnly? fromDate = null, DateOnly? toDate = null);

        // Statistics
        Task<int> GetTotalArchiveRecordsAsync();
        Task<DateTime> GetOldestArchiveDateAsync();
        Task<DateTime> GetLatestArchiveDateAsync();

        // Cleanup operations
        Task<int> DeleteOldArchiveAsync(DateOnly beforeDate);
    }
}