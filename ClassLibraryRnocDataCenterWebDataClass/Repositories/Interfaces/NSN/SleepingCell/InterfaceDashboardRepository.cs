using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN
{
    public interface InterfaceDashboardRepository : InterfaceBaseRepository<VKpiResultToday>
    {
        // Today's dashboard data
        Task<IEnumerable<VKpiResultToday>> GetTodayKpiSummaryAsync();
        Task<IEnumerable<VKpiResultToday>> GetTodayKpiByProvinceAsync(string province);
        Task<IEnumerable<VKpiResultToday>> GetTodayKpiByVendorAsync(string vendor);
        Task<IEnumerable<VKpiResultToday>> GetTodayKpiByRegionAsync(string region);

        // Archive dashboard data
        Task<IEnumerable<VKpiArchiveSummary>> GetArchiveKpiSummaryAsync(DateOnly fromDate, DateOnly toDate);
        Task<IEnumerable<VKpiArchiveSummary>> GetMonthlyKpiSummaryAsync(int year, int month);
        Task<IEnumerable<VKpiArchiveSummary>> GetQuarterlyKpiSummaryAsync(int year, int quarter);

        // Statistics aggregation
        Task<int> GetTotalSleepingCellsTodayAsync();
        Task<int> GetTotalCellsTodayAsync();
        Task<decimal> GetSleepingCellRateTodayAsync();
        Task<Dictionary<string, int>> GetSleepingCellsByProvinceAsync();
        Task<Dictionary<string, int>> GetSleepingCellsByVendorAsync();

        // Trend analysis
        Task<IEnumerable<VKpiArchiveSummary>> GetSleepingCellTrendAsync(int days = 30);
        Task<Dictionary<string, decimal>> GetProvincePerformanceTrendAsync(string province, int days = 7);
        Task<Dictionary<string, decimal>> GetVendorPerformanceTrendAsync(string vendor, int days = 7);

        // Alert data
        Task<IEnumerable<VKpiResultToday>> GetProblemsRequiringAttentionAsync();
        Task<bool> HasCriticalIssuesAsync();
        Task<int> GetHighPriorityAlertCountAsync();

        // Performance metrics
        Task<decimal> GetOverallNetworkHealthScoreAsync();
        Task<Dictionary<string, decimal>> GetRegionalHealthScoresAsync();
    }
}
