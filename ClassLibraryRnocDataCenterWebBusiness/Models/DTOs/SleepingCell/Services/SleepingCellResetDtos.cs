using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class ResetRequestDto
    {
        public string CellName { get; set; } = string.Empty;
        public string ExecutedBy { get; set; } = string.Empty;
        public bool ForceReset { get; set; } = false;
        public string Reason { get; set; } = string.Empty;
    }

    public class BatchResetRequestDto
    {
        public IEnumerable<string> CellNames { get; set; } = new List<string>();
        public string ExecutedBy { get; set; } = string.Empty;
        public bool ForceReset { get; set; } = false;
        public string Reason { get; set; } = string.Empty;
    }

    public class ResetResultDto
    {
        public string CellName { get; set; } = string.Empty;
        public string BtsName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public DateTime ResetTime { get; set; }
        public string ExecutedBy { get; set; } = string.Empty;
        public TimeSpan ExecutionDuration { get; set; }
        public string Vendor { get; set; } = string.Empty;
    }

    public class BatchResetResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public IEnumerable<ResetResultDto> Results { get; set; } = new List<ResetResultDto>();
        public string OverallStatus { get; set; } = string.Empty;
        public TimeSpan TotalExecutionTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ResetValidationDto
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> EligibleCells { get; set; } = new List<string>();
        public IEnumerable<string> BlacklistedCells { get; set; } = new List<string>();
        public IEnumerable<string> InvalidCells { get; set; } = new List<string>();
        public string ValidationMessage { get; set; } = string.Empty;
        public bool WithinDailyLimit { get; set; }
        public int RemainingQuota { get; set; }
        public bool WithinTimeWindow { get; set; }
        public string TimeWindowMessage { get; set; } = string.Empty;
    }
}
