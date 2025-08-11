using System;
using System.Collections.Generic;

namespace WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

/// <summary>
/// Bảng archive KPI data với support query linh hoạt theo ngày/tháng/địa điểm/tên
/// </summary>
public partial class Objtable4gkpireportresultarchive
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

    public DateTime? OriginalCreatedAt { get; set; }

    /// <summary>
    /// Tỉnh/Thành phố (extract từ site name)
    /// </summary>
    public string? Province { get; set; }

    public string? District { get; set; }

    /// <summary>
    /// Miền: North/Central/South
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Ngày của data KPI (để query theo thời gian)
    /// </summary>
    public DateOnly DataDate { get; set; }

    public int? DataYear { get; set; }

    public int? DataMonth { get; set; }

    public int? DataDay { get; set; }

    public int? DataQuarter { get; set; }

    public int? DataWeek { get; set; }

    public string? Vendor { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public string? ArchivedBy { get; set; }
}
