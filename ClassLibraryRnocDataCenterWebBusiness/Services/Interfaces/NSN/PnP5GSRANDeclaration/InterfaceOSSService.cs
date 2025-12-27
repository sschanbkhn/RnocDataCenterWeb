using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration
{
    public interface InterfaceOSSService
    {
        /// <summary>
        /// Upload XML files to OSS Netact
        /// </summary>
        Task<UploadToOSSResponse> UploadXMLFilesAsync(string jobId, string commissionType);

        /// <summary>
        /// Trigger commissioning job in OSS Netact
        /// </summary>
        Task<ExecuteCommissioningResponse> TriggerCommissioningAsync(string ossJobId, string commissionType);

        /// <summary>
        /// Get execution status (for polling)
        /// </summary>
        Task<ExecutionStatusResponse> GetExecutionStatusAsync(string executionId);

        /// <summary>
        /// Get final execution results
        /// </summary>
        Task<ExecutionResultsResponse> GetExecutionResultsAsync(string executionId);

        /// <summary>
        /// Get error report file
        /// </summary>
        Task<byte[]> GetErrorReportAsync(string executionId);

        /// <summary>
        /// Get execution logs file
        /// </summary>
        Task<byte[]> GetExecutionLogsAsync(string executionId);
    }
    // ====================================================================


    // ====================================================================
    // DTOs
    // ====================================================================

    public class UploadToOSSResponse
    {
        public int UploadedFiles { get; set; }
        public int FailedFiles { get; set; }
        public string OssJobId { get; set; }
    }

    public class ExecuteCommissioningResponse
    {
        public string ExecutionId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class ExecutionStatusResponse
    {
        public string ExecutionId { get; set; }
        public string Status { get; set; }
        public ExecutionProgress Progress { get; set; }
        public string Message { get; set; }

        public DateTime? CompletedAt { get; set; } // ← THÊM
        public DateTime? StartedAt { get; set; } // ← THÊM
        public List<SiteExecutionResult> SiteResults { get; set; } = new(); // ← THÊM


    }

    public class ExecutionProgress
    {
        public int Percentage { get; set; }
        public string CurrentSite { get; set; }
        public int CompletedSites { get; set; }
        public int FailedSites { get; set; }
        public int TotalSites { get; set; }
    }

    public class ExecutionResultsResponse
    {
        public string ExecutionId { get; set; }
        public string Status { get; set; }
        public int TotalSites { get; set; }
        public int SuccessfulSites { get; set; }
        public int FailedSites { get; set; }
        public string Duration { get; set; }
        public List<SiteExecutionResult> Results { get; set; }
        public DownloadLinks DownloadLinks { get; set; }
    }

    public class SiteExecutionResult
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Timing
        public DateTime? StartedAt { get; set; }    // ← Khi nào bắt đầu execute?
        // public DateTime? CompletedAt { get; set; }

        // Execution details (useful for troubleshooting)
        public string XmlFileName { get; set; }     // ← File XML nào?
        public string PlanName { get; set; }        // ← ZT_AUTO_... plan name
        public string CommandOutput { get; set; }   // ← Output từ racclimx.sh (để debug)
    


}

    public class DownloadLinks
    {
        public string XmlZip { get; set; }
        public string ErrorReport { get; set; }
        public string ExecutionLog { get; set; }
    }

}
