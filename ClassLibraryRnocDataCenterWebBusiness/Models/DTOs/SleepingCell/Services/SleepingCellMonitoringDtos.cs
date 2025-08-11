using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    class SleepingCellMonitoringDtos
    {
    }
    // <summary>
    /// Request model cho KPI Monitor API
    /// </summary>
    public class KpiMonitorRequest
    {
        public string Date { get; set; } = "";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Region { get; set; }
        public string? Vendor { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// Response record cho KPI Monitor - chỉ 13 cột cần thiết + STT
    /// </summary>
    public class KpiMonitorRecord
    {
        public long Id { get; set; }
        public int OriginalId { get; set; }
        public string? PeriodStartTime { get; set; }
        public string? MrbtsName { get; set; }
        public string? LnbtsName { get; set; }
        public string? LncelName { get; set; }
        public string? DnMrbtsSite { get; set; }
        public decimal? PdcpVolumeDl { get; set; }
        public decimal? PdcpVolumeUl { get; set; }
        public decimal? CellAvail { get; set; }
        public int? MaxUes { get; set; }
        public long? MaxPdcpDl { get; set; }
        public long? MaxPdcpUl { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Region { get; set; }
        public string? Vendor { get; set; }
        public DateTime? DataDate { get; set; }

        // Các field khác cho Detail modal
        public int? DataYear { get; set; }
        public int? DataMonth { get; set; }
        public int? DataDay { get; set; }
        public int? DataQuarter { get; set; }
        public int? DataWeek { get; set; }
        public DateTime? OriginalCreatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }

        public string? ExecutionStatus { get; set; }  // ← THÊM DÒNG NÀY

        // public string? ExecutionStatus { get; set; }  // ← THÊM DÒNG NÀY
    }

    /// <summary>
    /// Response wrapper cho KPI Monitor API
    /// </summary>
    public class KpiMonitorResponse
    {
        public bool Success { get; set; }
        public List<KpiMonitorRecord> Records { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public KpiMonitorFilters? Filters { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Filter options cho dropdowns
    /// </summary>
    public class KpiMonitorFilters
    {
        public List<string> Provinces { get; set; } = new();
        public List<string> Districts { get; set; } = new();
        public List<string> Regions { get; set; } = new();
        public List<string> Vendors { get; set; } = new();
    }




    public class KpiMonitorDateRangeRequest
    {
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Region { get; set; }
        public string? Vendor { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }



}
