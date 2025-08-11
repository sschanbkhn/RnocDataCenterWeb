using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class SleepingCellDto
    {
        public string CellName { get; set; } = string.Empty;
        public string BtsName { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public decimal TrafficDl { get; set; }
        public decimal TrafficUl { get; set; }
        public decimal Availability { get; set; }
        public DateTime LastDetected { get; set; }
        public bool IsEligibleForReset { get; set; }
    }

    public class SleepingCellStatsDto
    {
        public int TotalSleepingCells { get; set; }
        public int TotalCells { get; set; }
        public decimal SleepingCellRate { get; set; }
        public Dictionary<string, int> ByProvince { get; set; } = new();
        public Dictionary<string, int> ByVendor { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class CellStatusDto
    {
        public string CellName { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public bool IsSleeping { get; set; }
        public bool IsBlacklisted { get; set; }
        public bool IsResetAllowed { get; set; }
        public DateTime? LastResetTime { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }
}