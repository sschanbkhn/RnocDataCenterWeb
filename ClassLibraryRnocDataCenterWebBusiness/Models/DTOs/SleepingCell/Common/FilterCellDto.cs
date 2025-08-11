// File: ClassLibraryRnocDataCenterWebBusiness/Models/DTOs/SleepingCell/Common/FilterCellDto.cs

using System;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common
{
    public class FilterCellDto
    {
        public string CellName { get; set; } = string.Empty;
        public string BtsName { get; set; } = string.Empty;
        public decimal? Availability { get; set; }
        public decimal? TrafficDl { get; set; }
        public decimal? TrafficUl { get; set; }
        public string PeriodStartTime { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}