using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

/// <summary>
/// Bảng giới hạn số sites được reset mỗi ngày để bảo vệ hệ thống
/// </summary>
public partial class Objtableresetsitecountlimit
{
    public int Id { get; set; }

    public DateOnly LimitDate { get; set; }

    /// <summary>
    /// Tối đa số sites được reset trong 1 ngày
    /// </summary>
    public int MaxSitesPerDay { get; set; }

    /// <summary>
    /// Số sites đã reset hôm nay
    /// </summary>
    public int? SitesResetToday { get; set; }

    public DateTime? LastResetTime { get; set; }

    public int? MaxNsnSites { get; set; }

    public int? MaxEricssonSites { get; set; }

    public int? MaxHuaweiSites { get; set; }

    public int? NsnSitesReset { get; set; }

    public int? EricssonSitesReset { get; set; }

    public int? HuaweiSitesReset { get; set; }

    /// <summary>
    /// Bật/tắt chức năng auto reset
    /// </summary>
    public bool? AutoResetEnabled { get; set; }

    /// <summary>
    /// Cho phép vượt limit trong trường hợp khẩn cấp
    /// </summary>
    public bool? EmergencyOverride { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
