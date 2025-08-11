using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class ValidationRequestDto
    {
        public IEnumerable<string> CellNames { get; set; } = new List<string>();
        public string RequestedBy { get; set; } = string.Empty;
        public bool ForceValidation { get; set; } = false;
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
        public string ValidationLevel { get; set; } = "Standard"; // Standard, Strict, Basic
    }

    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> ValidCells { get; set; } = new List<string>();
        public IEnumerable<string> InvalidCells { get; set; } = new List<string>();
        public IEnumerable<ValidationErrorDetailDto> ValidationErrors { get; set; } = new List<ValidationErrorDetailDto>();
        public string OverallMessage { get; set; } = string.Empty;
        public DateTime ValidationTime { get; set; } = DateTime.UtcNow;
        public TimeSpan ValidationDuration { get; set; }
    }

    public class ValidationErrorDetailDto
    {
        public string CellName { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
        public object? AdditionalInfo { get; set; }
    }

    public class BusinessRuleValidationDto
    {
        public bool PassedBlacklistCheck { get; set; }
        public bool PassedDailyLimitCheck { get; set; }
        public bool PassedTimeWindowCheck { get; set; }
        public bool PassedVendorLimitCheck { get; set; }
        public bool PassedResetCooldownCheck { get; set; }
        public IEnumerable<string> FailedRules { get; set; } = new List<string>();
        public string ValidationSummary { get; set; } = string.Empty;
        public Dictionary<string, object> RuleDetails { get; set; } = new();
    }

    public class SystemValidationDto
    {
        public bool CellExists { get; set; }
        public bool BtsExists { get; set; }
        public bool HasValidConfiguration { get; set; }
        public bool IsSystemHealthy { get; set; }
        public bool SshConnectionAvailable { get; set; }
        public IEnumerable<string> SystemIssues { get; set; } = new List<string>();
        public string SystemStatus { get; set; } = string.Empty;
    }

    public class PreResetCheckDto
    {
        public string CellName { get; set; } = string.Empty;
        public bool ReadyForReset { get; set; }
        public BusinessRuleValidationDto BusinessRules { get; set; } = new();
        public SystemValidationDto SystemChecks { get; set; } = new();
        public IEnumerable<string> Prerequisites { get; set; } = new List<string>();
        public IEnumerable<string> Warnings { get; set; } = new List<string>();
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class BatchValidationDto
    {
        public int TotalRequested { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
        public int WarningCount { get; set; }
        public IEnumerable<PreResetCheckDto> CellChecks { get; set; } = new List<PreResetCheckDto>();
        public string BatchStatus { get; set; } = string.Empty;
        public string BatchSummary { get; set; } = string.Empty;
    }
}
