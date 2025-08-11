using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

/// <summary>
/// Detailed cell information with complete reset tracking, execution logs, and sync with infor table
/// </summary>
public partial class Objtable4gkpireportresultdetail
{
    public long Id { get; set; }

    public int? OriginalId { get; set; }

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

    public DateOnly? DataDate { get; set; }

    public int? DataYear { get; set; }

    public int? DataMonth { get; set; }

    public int? DataDay { get; set; }

    public int? DataQuarter { get; set; }

    public int? DataWeek { get; set; }

    public string? Vendor { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public DateTime? OriginalCreatedAt { get; set; }

    public string? ArchivedBy { get; set; }

    public bool? ActionBlacklist { get; set; }

    public string? UserNotes { get; set; }

    public bool? ResetPermission { get; set; }

    public int? MrbtsInforId { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public int? ResetCount { get; set; }

    public DateTime? LastResetAt { get; set; }

    public string? LastResetBy { get; set; }

    public bool? LastResetSuccess { get; set; }

    public decimal? TotalSuccessRate { get; set; }

    public bool? ResetEnabled { get; set; }

    /// <summary>
    /// JSON array storing all reset attempts with complete details
    /// </summary>
    public string? ResetHistory { get; set; }

    public string? ExecutionNotes { get; set; }

    public string? ExecutionStatus { get; set; }

    public DateTime? ExecutionStartedAt { get; set; }

    public DateTime? ExecutionCompletedAt { get; set; }

    public TimeSpan? ExecutionDuration { get; set; }

    public string? SshHost { get; set; }

    public string? SshConnectionStatus { get; set; }

    /// <summary>
    /// JSON object storing detailed execution steps and SSH transcript
    /// </summary>
    public string? ExecutionLog { get; set; }

    public bool? PingTestBefore { get; set; }

    public bool? PingTestAfter { get; set; }

    public DateTime? SshConnectStartedAt { get; set; }

    public DateTime? SshConnectCompletedAt { get; set; }

    public DateTime? CommandSentAt { get; set; }

    public DateTime? CommandResponseReceivedAt { get; set; }

    public virtual ObjtablemrbtsInfor? MrbtsInfor { get; set; }
}
