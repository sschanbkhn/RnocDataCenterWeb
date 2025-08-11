using System;

namespace ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services
{
    public class BulkResetFromFilterTableResultDto
    {
        public bool Success { get; set; }
        public int TotalCells { get; set; }
        public int SuccessfulResets { get; set; }
        public int FailedResets { get; set; }
        public string BatchId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string ExecutedBy { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}