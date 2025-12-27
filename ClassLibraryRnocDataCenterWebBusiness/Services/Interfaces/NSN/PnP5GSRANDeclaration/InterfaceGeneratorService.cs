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


}
