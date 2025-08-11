using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class DashboardSummaryDto
    {
        public SleepingCellStatsDto SleepingCellStats { get; set; } = new();
        public DailyLimitConfigDto DailyLimits { get; set; } = new();
        public IEnumerable<AlertDto> ActiveAlerts { get; set; } = new List<AlertDto>();
        public NetworkHealthDto NetworkHealth { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public bool SystemStatus { get; set; }
    }

    public class NetworkHealthDto
    {
        public decimal OverallHealthScore { get; set; }
        public Dictionary<string, decimal> VendorHealthScores { get; set; } = new();
        public Dictionary<string, decimal> ProvinceHealthScores { get; set; } = new();
        public IEnumerable<string> CriticalIssues { get; set; } = new List<string>();
        public DateTime LastCalculated { get; set; }
    }

    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public bool IsAcknowledged { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
    }

    public class TrendAnalysisDto
    {
        public Dictionary<string, decimal> DailyTrends { get; set; } = new();
        public Dictionary<string, decimal> WeeklyTrends { get; set; } = new();
        public Dictionary<string, decimal> MonthlyTrends { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public decimal ChangePercentage { get; set; }
        public DateTime AnalysisDate { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public int TotalResetOperations { get; set; }
        public decimal SuccessRate { get; set; }
        public TimeSpan AverageResetTime { get; set; }
        public Dictionary<string, int> ResetsByVendor { get; set; } = new();
        public Dictionary<string, int> ResetsByProvince { get; set; } = new();
        public DateTime MetricsPeriodStart { get; set; }
        public DateTime MetricsPeriodEnd { get; set; }
    }

    public class ReportRequestDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public IEnumerable<string> Vendors { get; set; } = new List<string>();
        public IEnumerable<string> Provinces { get; set; } = new List<string>();
        public string Format { get; set; } = "json";
        public string RequestedBy { get; set; } = string.Empty;
    }

    // phan tren chua dung

    public class Zone1Response
    {
        public string Date { get; set; }
        public int TodayAnalysis { get; set; }
        public int SleepingCells { get; set; }

        public int ProcessCells_ { get; set; }

        public int ExecutionCells { get; set; }
        public int RecheckCells { get; set; }
    }




    public class SleepingCellTrendResponse
    {
        public bool Success { get; set; }
        public List<SleepingCellDayData> Data { get; set; }
        public SleepingCellSummary Summary { get; set; }
    }

    public class SleepingCellDayData
    {
        public string Date { get; set; }
        public int TodayAnalysis { get; set; }
        public int SleepingCells { get; set; }
        public int ProcessCells_ { get; set; }  // Giữ tên này để match với frontend
        public int ExecutionCells { get; set; }
        public int RecheckCells { get; set; }
        public double SuccessRate { get; set; }
    }

    public class SleepingCellSummary
    {
        public int TotalDays { get; set; }
        public double AvgSleepingCells { get; set; }
        public double AvgSuccessRate { get; set; }
    }



}