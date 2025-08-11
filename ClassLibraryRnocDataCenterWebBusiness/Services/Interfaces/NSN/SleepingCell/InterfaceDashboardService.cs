using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell
{
    public interface InterfaceDashboardService
    {
        // Dashboard summary for N8N
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<NetworkHealthDto> GetNetworkHealthAsync();

        // Performance metrics
        Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate);
        Task<TrendAnalysisDto> GetTrendAnalysisAsync(int days = 30);

        // Alerts and notifications
        Task<IEnumerable<AlertDto>> GetActiveAlertsAsync();
        Task<IEnumerable<AlertDto>> GetAlertHistoryAsync(int days = 7);

        // Reporting
        Task<string> GenerateReportAsync(ReportRequestDto request);
        Task<byte[]> GenerateReportFileAsync(ReportRequestDto request);

        // Real-time statistics for N8N monitoring
        Task<SleepingCellStatsDto> GetRealTimeStatsAsync();
        Task<Dictionary<string, object>> GetSystemStatusAsync();

        // Configuration monitoring
        Task<IEnumerable<string>> GetSystemIssuesAsync();
        Task<bool> IsSystemHealthyAsync();


        // phan tren chua dung
        Task<Zone1Response> GetSummaryAsync(DateOnly date);

        // Task<SleepingCellTrendResponse> GetTrendAsync();
        // Trong InterfaceDashboardService, sửa dòng này:
        // Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null);
        // Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null);

        Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null);

        Task<object> funDashboardServiceGetProvinceSummary(string date);
        // Task<object> funDashboardServiceGetProvinceSummary();  // object thay vì ActionResult

        Task<object> funDashboardServiceGetZone4SummaryAsync(string date);



        // Trong InterfaceDashboardService.cs, thêm:
        Task<object> funDashboardServiceGetSleepingCellsAsync(string date);
        Task<object> funDashboardServiceGetProcessCellsAsync(string date);
        Task<object> funDashboardServiceGetExecutionCellsAsync(string date);
        Task<object> funDashboardServiceGetRecheckCellsAsync(string date);



    }
}