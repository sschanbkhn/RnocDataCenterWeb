using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell
{
    /// class InterfaceMonitorService
    /// {
    /// }

    /// <summary>
    /// Interface for KPI Monitor Service - Simple implementation with only Detail button
    /// </summary>
    public interface InterfaceMonitorService
    {
        /// <summary>
        /// Get KPI Monitor data with pagination and filters
        /// </summary>
        /// <param name="request">Request parameters including date, pagination, filters, search, sort</param>
        /// <returns>Response object with records, pagination info, and filter options</returns>
        Task<object> funMonitorServiceGetKpiMonitorDataAsync(KpiMonitorRequest request);

        /// <summary>
        /// Get filter options for dropdowns (provinces, districts, regions, vendors)
        /// </summary>
        /// <returns>Filter options object</returns>
        Task<object> funMonitorServiceGetFiltersAsync();

        /// <summary>
        /// Export KPI Monitor data to Excel file
        /// </summary>
        /// <param name="date">Target date in YYYY-MM-DD format</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> funMonitorServiceExportToExcelAsync(string date, KpiMonitorRequest? filters = null);


        // Thêm vào InterfaceMonitorService
        Task<object> funMonitorServiceGetKpiMonitorDataRangeAsync(KpiMonitorDateRangeRequest request);


        Task<object> funMonitorServiceGetKpiMonitorDataSelectCellDetail(string cellName, string date);

    }

}
