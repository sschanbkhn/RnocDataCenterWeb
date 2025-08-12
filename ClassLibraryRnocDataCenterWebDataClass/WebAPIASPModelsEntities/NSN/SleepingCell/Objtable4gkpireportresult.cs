using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

/// <summary>
/// Bảng kết quả reset ngày hôm nay - cùng structure với archive table
/// </summary>
public partial class Objtable4gkpireportresult
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

    /// <summary>
    /// Ngày của data (thường là CURRENT_DATE)
    /// </summary>
    public DateOnly DataDate { get; set; }

    public int? DataYear { get; set; }

    public int? DataMonth { get; set; }

    public int? DataDay { get; set; }

    public int? DataQuarter { get; set; }

    public int? DataWeek { get; set; }

    public string? Vendor { get; set; }

    /// <summary>
    /// Thời gian xử lý (processing time)
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    public DateTime? OriginalCreatedAt { get; set; }

    public string? ArchivedBy { get; set; }

    public string? ExecutionStatus { get; set; }
}
