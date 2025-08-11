using System;
using System.Collections.Generic;

namespace WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

public partial class VKpiArchiveSummary
{
    public DateOnly? DataDate { get; set; }

    public int? DataYear { get; set; }

    public int? DataMonth { get; set; }

    public int? DataQuarter { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Region { get; set; }

    public string? Vendor { get; set; }

    public long? TotalCells { get; set; }

    public long? SleepingCells { get; set; }

    public decimal? AvgDlTraffic { get; set; }

    public decimal? AvgUlTraffic { get; set; }

    public decimal? AvgAvailability { get; set; }

    public decimal? TotalDlTraffic { get; set; }

    public decimal? TotalUlTraffic { get; set; }
}
