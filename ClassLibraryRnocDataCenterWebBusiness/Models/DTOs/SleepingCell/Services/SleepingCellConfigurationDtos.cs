using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class DailyLimitConfigDto
    {
        public int MaxDailyLimit { get; set; }
        public int UsedToday { get; set; }
        public int RemainingQuota { get; set; }
        public decimal UsagePercentage { get; set; }
        public bool CanResetMore { get; set; }
        public bool AutoResetEnabled { get; set; }
        public DateTime LastResetTime { get; set; }
        public Dictionary<string, VendorLimitDto> VendorLimits { get; set; } = new();
    }

    public class VendorLimitDto
    {
        public string Vendor { get; set; } = string.Empty;
        public int MaxLimit { get; set; }
        public int Used { get; set; }
        public int Remaining { get; set; }
        public decimal UsagePercentage { get; set; }
        public bool CanResetMore { get; set; }
    }

    public class BtsConfigDto
    {
        public string BtsName { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; }
        public bool IsResetAllowed { get; set; }
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class BlacklistConfigDto
    {
        public IEnumerable<string> BlacklistedBts { get; set; } = new List<string>();
        public IEnumerable<string> WhitelistedBts { get; set; } = new List<string>();
        public string Reason { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class TimeWindowConfigDto
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public IEnumerable<string> AllowedDays { get; set; } = new List<string>();
        public string TimeZone { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
    }

    public class SshAccountConfigDto
    {
        public string System { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastUsed { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateConfigRequestDto
    {
        public string ConfigType { get; set; } = string.Empty;
        public Dictionary<string, object> Settings { get; set; } = new();
        public string UpdatedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}