using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration
{
    public interface InterfaceGeneratorService
    {
        /// <summary>
        /// Get template content by filename
        /// </summary>
        Task<string> GetTemplateContentAsync(string commissionType, string fileName);

        /// <summary>
        /// Generate XML files from templates and site data
        /// </summary>
        Task<GenerateXMLResponse> GenerateXMLFilesAsync(GenerateXMLRequest request);

        /// <summary>
        /// Get ZIP file containing generated XMLs
        /// </summary>
        Task<byte[]> GetXMLZipAsync(string jobId);



        /// <summary>
        /// Đọc nội dung file XML đã generate
        /// </summary>
        /// <param name="jobId">Job ID (VD: job_20251217_141845)</param>
        /// <param name="fileName">Tên file XML (VD: 5G-CGY017M-HNI.xml)</param>
        /// <returns>Nội dung XML dạng text</returns>
        Task<string> GetXMLFileContentAsync(string jobId, string fileName);


        /// <summary>
        /// Đọc file XML dạng binary để download
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="fileName">Tên file XML</param>
        /// <returns>File data dạng byte array</returns>
        Task<byte[]> GetSingleXMLFileAsync(string jobId, string fileName); // ← THÊM

        // ====================================================================
        // NEW - Database methods
        // ====================================================================
        /// <summary>
        /// Mục đích: Lưu job mới vào database
        /// Input: Thông tin job (loại, file, số sites)
        /// Output: job_id (BIGINT)
        /// </summary>
        Task<long> SaveJobAsync(string jobType, string inputFileName, string inputFilePath, int totalSites, string createdBy = "admin" );
        // ====================================================================

        /// <summary>
        /// Mục đích: Bulk insert tất cả sites từ Excel vào validation table
        /// Input: job_id, danh sách sites (62 columns)
        /// Output: void
        /// </summary>
        Task SaveValidationSitesAsync( long jobId, List<SiteValidationData> sites );
        // ====================================================================

        /// <summary>
        /// Mục đích: Bulk insert kết quả template matching
        /// Input: job_id, danh sách template matching (23 columns)
        /// Output: void
        /// </summary>
        Task SaveTemplateMatchingAsync( long jobId, List<SiteTemplateData> templates);
        // ====================================================================

        /// <summary>
        /// Mục đích: Lưu kết quả XML generation (support retry)
        /// Input: job_id, danh sách generation results
        /// Output: void
        /// </summary>
        Task SaveGenerationResultsAsync(long jobId, List<SiteGenerationData> generation);
        // ====================================================================


        /// <summary>
        /// Mục đích: Lưu kết quả OSS execution (support retry)
        /// Input: job_id, danh sách OSS execution results
        /// Output: void
        /// </summary>
        Task SaveOssExecutionAsync(long jobId, List<SiteOssExecutionData> executions);
        // ====================================================================

        /// <summary>
        /// Mục đích: Cập nhật thống kê job sau khi hoàn thành
        /// Input: job_id, thống kê các bước (validation, matching, generation, OSS)
        /// Output: void
        /// </summary>
        Task UpdateJobStatisticsAsync(long jobId, UpdateJobStatisticsData statistics);


    }
    //========================================================================


    // DTOs for this service
    public class GenerateXMLRequest
    {
        public string CommissionType { get; set; }
        public List<SiteDataDTO> Sites { get; set; }
    }
    //========================================================================

    public class SiteDataDTO
    {
        public string Id { get; set; }
        public string SiteName { get; set; }
        public string TemplateId { get; set; }
        public string IpOam { get; set; }
        public string Band5g { get; set; }
        public string Rrh5g { get; set; }
        // ... (thêm fields khác nếu cần)
    }
    //========================================================================

    public class GenerateXMLResponse
    {
        public string JobId { get; set; }
        public int TotalSites { get; set; }
        public List<GeneratedFileResult> GeneratedFiles { get; set; }
    }
    //========================================================================

    public class GeneratedFileResult
    {
        public string SiteId { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }
    //========================================================================



    // ====================================================================
    // DTO cho validation sites (62 columns)
    // ====================================================================
    public class SiteValidationData
    {
        // System columns
        public int SiteOrder { get; set; }
        public string ValidationStatus { get; set; } = "Valid";
        public string ValidationErrors { get; set; }

        // Mapping columns (extracted từ mrbts_name)
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }

        // Excel columns (54 columns)
        public int? Stt { get; set; }
        public string MrbtsName { get; set; }
        public string Address { get; set; }
        public string Oam { get; set; }
        public string Men { get; set; }
        public string Tac { get; set; }
        public string Province { get; set; }
        public string District { get; set; }

        public string SerialSm5g { get; set; }
        public string NrbtsId5g { get; set; }
        public string MrbtsId { get; set; }
        public string MrbtsName5g { get; set; }
        public string MrbtsName4g { get; set; }

        public string VlanMsPlane5g { get; set; }
        public string IpMsPlane5g { get; set; }
        public string GatewayIpMsPlane5g { get; set; }
        public string VlanCuPlane5g { get; set; }
        public string IpCuPlane5g { get; set; }
        public string GatewayIpCuPlane5g { get; set; }

        public string IpMsPlane4g { get; set; }
        public string IpCuPlane4g { get; set; }

        public string NetactOms { get; set; }
        public string PrimaryDns { get; set; }
        public string SecondaryDns { get; set; }
        public string DcapIp { get; set; }
        public string NetactOam { get; set; }

        public int? PhysCellId1 { get; set; }
        public int? PhysCellId2 { get; set; }
        public int? PhysCellId3 { get; set; }
        public int? RootSeqIndex1 { get; set; }
        public int? RootSeqIndex2 { get; set; }
        public int? RootSeqIndex3 { get; set; }
        public string Beamset1 { get; set; }
        public string Beamset2 { get; set; }
        public string Beamset3 { get; set; }
        public int? TiltOffset1 { get; set; }
        public int? TiltOffset2 { get; set; }
        public int? TiltOffset3 { get; set; }
        public string Profile { get; set; }
        public string FbbManager { get; set; }
        public string ManagementCell5g { get; set; }

        public string Band5g { get; set; }
        public string Band4g { get; set; }
        public string Sm { get; set; }
        public string Rrh5g { get; set; }
        public string Rrh4g { get; set; }
        public string PortInformation { get; set; }
        public string Nrarfcn { get; set; }
        public string ChannelBandwidth { get; set; }

        public string Rmod5g { get; set; }
        public string Rmod4g { get; set; }
        public string Antl2g { get; set; }
        public string Antl3g { get; set; }
        public string Antl4g { get; set; }
    }
    // ====================================================================

    // ====================================================================
    // DTO cho template matching (23 columns)
    // ====================================================================
    public class SiteTemplateData
    {
        // System columns
        public long ValidationId { get; set; }

        // Matching results (6 columns)
        public string MatchStatus { get; set; } = "Matched";
        public string MatchedTemplateName { get; set; }
        public string TemplateFilePath { get; set; }
        public string MatchMethod { get; set; } = "Auto";
        public string MatchReason { get; set; }

        // Site info duplicated from validation (17 columns)
        public int? Stt { get; set; }
        public string MrbtsName { get; set; }
        public string Address { get; set; }
        public string IpOam { get; set; }
        public string Province { get; set; }
        public string District { get; set; }

        public string SerialSm5g { get; set; }
        public string NrbtsId5g { get; set; }
        public string MrbtsId { get; set; }
        public string MrbtsName5g { get; set; }

        public string Profile { get; set; }
        public string FbbManager { get; set; }
        public string ManagementCell5g { get; set; }
        public string Sm { get; set; }
        public string Band5g { get; set; }
        public string Rrh5g { get; set; }
        public string ChannelBandwidth { get; set; }
    }
    // ====================================================================


    // ====================================================================
    // DTO cho generation results
    // ====================================================================
    public class SiteGenerationData
    {
        // Foreign keys
        public long ValidationId { get; set; }
        public long TemplateId { get; set; }

        // Retry support
        public int AttemptNumber { get; set; } = 1;

        // Generation results
        public string GenerationStatus { get; set; } = "Success";  // Success / Failed
        public string XmlFileName { get; set; }
        public string XmlFilePath { get; set; }
        public string ErrorMessage { get; set; }

        // Timestamps
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int? DurationSeconds { get; set; }
    }
    // ====================================================================


    // ====================================================================
    // DTO cho OSS execution results
    // ====================================================================
    public class SiteOssExecutionData
    {
        public long GenerationId { get; set; }
        public int AttemptNumber { get; set; } = 1;
        public string OssFilePath { get; set; }
        public string OssCommand { get; set; }
        public string OssResponse { get; set; }
        public string UploadStatus { get; set; } = "Success";
        public string ExecutionStatus { get; set; } = "Success";
        public DateTime? UploadedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string ErrorMessage { get; set; }
    }
    // ====================================================================



    // ====================================================================
    // DTO cho job statistics update
    // ====================================================================
    public class UpdateJobStatisticsData
    {
        // Site statistics
        public int? TotalSites { get; set; }
        public int? ValidSites { get; set; }
        public int? InvalidSites { get; set; }
        public int? MatchedSites { get; set; }
        public int? UnmatchedSites { get; set; }
        public int? GeneratedSuccessSites { get; set; }
        public int? GeneratedFailedSites { get; set; }
        public int? OssSuccessSites { get; set; }
        public int? OssFailedSites { get; set; }

        // Job status
        public string Status { get; set; }  // "Completed", "Failed", "Partial"

        // Timestamps
        public DateTime? CompletedAt { get; set; }
        public int? DurationSeconds { get; set; }

        // Error message
        public string ErrorMessage { get; set; }
    }
    // ====================================================================



}
