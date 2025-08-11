using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

public partial class Objtableresetsitesleepingcell
{
    public int Id { get; set; }

    public string? PeriodStartTime { get; set; }

    public string? MrbtsName { get; set; }

    public string? LnbtsName { get; set; }

    public string? LncelName { get; set; }

    public string? DnMrbtsSite { get; set; }

    public decimal? PdcpVolumeDl { get; set; }

    public decimal? PdcpVolumeUl { get; set; }

    public decimal? CellAvail { get; set; }

    public int? MaxUes { get; set; }

    public decimal? MaxPdcpDl { get; set; }

    public decimal? MaxPdcpUl { get; set; }

    public DateTime? CreatedAt { get; set; }
}
