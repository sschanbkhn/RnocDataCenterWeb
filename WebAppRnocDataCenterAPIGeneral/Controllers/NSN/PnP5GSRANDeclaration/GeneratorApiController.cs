using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.PnP5GSRANDeclaration
{
    [ApiController]
    [Route("api/pnp5Gsran-declaration/generator")]
    [Produces("application/json")]

    public class PnP5GGeneratorApiController : ControllerBase
    {

        private readonly string _templateFolderPath;
        private readonly IWebHostEnvironment _env;
        //========================================================================

        private readonly InterfaceGeneratorService _generatorService;
        private readonly InterfaceOSSService _ossService;
        //========================================================================


        public PnP5GGeneratorApiController(IWebHostEnvironment env,
    InterfaceGeneratorService generatorService,
    InterfaceOSSService ossService)
        {
            _env = env;
            // _env = env;
            _generatorService = generatorService;
            _ossService = ossService;
            // Path: Controllers/NSN/PnP5GSRANDeclaration/ConfigPnP
            /*
            _templateFolderPath = Path.Combine(
                _env.ContentRootPath,
                "Controllers",
                "NSN",
                "PnP5GSRANDeclaration",
                "ConfigPnP"
            );
            */

            // ✅ DÙNG StackTrace như R003:
            var currentFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            var controllerFolder = Path.GetDirectoryName(currentFilePath);

            _templateFolderPath = Path.Combine(controllerFolder, "ConfigPnP");
        }
        //========================================================================

        /*

        public PnP5GGeneratorApiController(
            InterfaceGeneratorService generatorService,
    InterfaceOSSService ossService)
        {
            _generatorService = generatorService;
            _ossService = ossService;
        }

        */

        /// <summary>
        /// Lấy danh sách tất cả templates
        /// GET /api/pnp5Gsran-declaration/r007/template/list
        /// </summary>
        [HttpGet("list-PnP")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetTemplateList()
        {
            try
            {
                if (!Directory.Exists(_templateFolderPath))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ConfigPnP folder not found",
                        path = _templateFolderPath
                    });
                }

                var files = Directory.GetFiles(_templateFolderPath, "*.xml")
                    .Select(f => new
                    {
                        id = Path.GetFileNameWithoutExtension(f),
                        fileName = Path.GetFileName(f),
                        // band = ExtractBand(Path.GetFileName(f)),
                        // rrh = ExtractRRH(Path.GetFileName(f)),
                        // sectors = ExtractSectors(Path.GetFileName(f))
                    })
                    .OrderBy(f => f.fileName)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    count = files.Count,
                    data = files
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        //========================================================================


        /// <summary>
        /// Download template file
        /// GET /api/pnp5Gsran-declaration/r007/template/download/{fileName}
        /// </summary>
        [HttpGet("download-pnp/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DownloadTemplate(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_templateFolderPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Template file not found"
                    });
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/xml", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        //========================================================================

        //========================================================================
        #region Helper Methods

        private string ExtractBand(string fileName)
        {
            var parts = fileName.Split('_');
            var bandPart = parts.FirstOrDefault(p => p.StartsWith("N") && p.Length >= 2 && p.Length <= 3);
            return bandPart ?? "";
        }
        //========================================================================

        private string ExtractRRH(string fileName)
        {
            var withoutExt = Path.GetFileNameWithoutExtension(fileName);
            var parts = withoutExt.Split('_');
            var lastPart = parts.LastOrDefault() ?? "";
            return new string(lastPart.SkipWhile(char.IsDigit).ToArray());
        }
        //========================================================================

        private string ExtractSectors(string fileName)
        {
            var parts = fileName.Split('_');
            if (parts.Length > 2)
            {
                var sectorPart = parts[2];
                if (sectorPart.Length == 3 && sectorPart.All(char.IsDigit))
                {
                    return sectorPart;
                }
            }
            return "";
        }
        //========================================================================


        public class UploadToOSSRequest
        {
            public string JobId { get; set; }
            public string CommissionType { get; set; }
        }

        #endregion
        //========================================================================

        //========================================================================
        #region phan API cho PnP

        /// <summary>
        /// API 1: Get Template Content
        /// GET /api/pnp5Gsran-declaration/generator/template-content/{fileName}
        /// Mục đích: Lấy nội dung XML của template file để preview hoặc edit
        // Input: fileName (string) - Tên file template (VD: "PnP_scf_..._N77_AVQG.xml")
        // Output: Nội dung XML dạng text
        // Workflow: 
        //   - Frontend user click "Preview Template" 
        //   - Gọi API này để xem nội dung template
        //   - Hiển thị XML trên màn hình
        // Ví dụ: GET /api/.../template-content/PnP_scf_..._N77_AVQG.xml
        // ====================================================================
        /// </summary>
        [HttpGet("template-content/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTemplateContent(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_templateFolderPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Template file not found"
                    });
                }

                var content = await System.IO.File.ReadAllTextAsync(filePath);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fileName = fileName,
                        content = content
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading template content",
                    error = ex.Message
                });
            }
        }
        //========================================================================


        /// <summary>
        /// API 2: Generate XML Files
        /// POST /api/pnp5Gsran-declaration/generator/generate-xml
        /// </summary>
        // ====================================================================
        // API 2: GENERATE XML FILES
        // ====================================================================
        // Mục đích: Merge template XML với site data → tạo file XML cho từng site
        // Input: 
        //   - CommissionType: "AutoPnP" hoặc "SiteConfig"
        //   - Sites: Array chứa 52 sites (mỗi site có templateId, siteName, ipOam...)
        // Output: 
        //   - jobId: "job_20241214_001" (unique ID cho batch này)
        //   - TotalSites: 52
        //   - GeneratedFiles: [list các file đã tạo + status success/failed]
        // Workflow:
        //   1. User ở Step 5 click "Start Generation"
        //   2. Gọi API này với 52 sites
        //   3. Backend tạo folder: GeneratedXML/job_20241214_001/
        //   4. Loop qua 52 sites:
        //      - Load template XML
        //      - Replace {{SITE_NAME}}, {{IP_OAM}}... bằng data thật
        //      - Save file: MRBTS-1600005.xml, MRBTS-1600006.xml...
        //   5. Return jobId để dùng cho các API sau
        // Kết quả: Folder chứa 52 file XML đã điền data sẵn sàng upload
        // ====================================================================
        [HttpPost("generate-xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateXML([FromBody] GenerateXMLRequest request)
        {
            try
            {
                var result = await _generatorService.GenerateXMLFilesAsync(request);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating XML files",
                    error = ex.Message
                });
            }
        }
        //========================================================================

        /// <summary>
        /// API 3: Download XML ZIP
        /// GET /api/pnp5Gsran-declaration/generator/download/{jobId}
        /// </summary>
        // ====================================================================
        // API 3: DOWNLOAD XML ZIP
        // ====================================================================
        // Mục đích: Nén tất cả XML files thành 1 file ZIP cho user download về máy
        // Input: jobId (string) - VD: "job_20241214_001"
        // Output: File ZIP (binary)
        // Workflow:
        //   1. User ở Step 5 hoặc Step 6 click "Download XML Files"
        //   2. Gọi API này với jobId từ API 2
        //   3. Backend zip folder GeneratedXML/job_20241214_001/
        //   4. Return file ZIP
        //   5. Browser tự động download: job_20241214_001.zip
        // Use case: User muốn backup hoặc kiểm tra XML trước khi upload OSS
        // ====================================================================

        [HttpGet("download/{jobId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadXMLZip(string jobId)
        {
            try
            {
                var zipData = await _generatorService.GetXMLZipAsync(jobId);

                return File(
                    zipData,
                    "application/zip",
                    $"{jobId}.zip"
                );
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error downloading ZIP file",
                    error = ex.Message
                });
            }
        }
        //========================================================================

        /// <summary>
        /// API 4: Upload to OSS Netact
        /// POST /api/pnp5Gsran-declaration/generator/upload-to-oss
        /// </summary>


        // ====================================================================
        // API 4: UPLOAD TO OSS NETACT
        // ====================================================================
        // Mục đích: Upload tất cả XML files lên OSS Netact server
        // Input:
        //   - jobId: "job_20241214_001"
        //   - commissionType: "AutoPnP"
        // Output:
        //   - uploadedFiles: 52 (số file upload thành công)
        //   - failedFiles: 0 (số file lỗi)
        //   - ossJobId: "oss_job_123" (ID từ OSS Netact để track)
        // Workflow:
        //   1. Sau khi generate XML xong (API 2)
        //   2. Backend tự động gọi API này (hoặc user click "Upload to OSS")
        //   3. Loop qua 52 XML files trong folder
        //   4. Upload từng file lên OSS Netact qua HTTP/FTP
        //   5. OSS Netact nhận files và tạo ossJobId
        //   6. Return ossJobId để dùng cho API 5
        // Lưu ý: Cần config OSS Netact URL, credentials
        // ====================================================================

        [HttpPost("upload-to-oss")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadToOSS([FromBody] UploadToOSSRequest request)
        {
            try
            {
                var result = await _ossService.UploadXMLFilesAsync(
                    request.JobId,
                    request.CommissionType);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error uploading to OSS Netact",
                    error = ex.Message
                });
            }
        }
        //========================================================================


        /// <summary>
        /// API 5: Execute Commissioning Job
        /// POST /api/pnp5Gsran-declaration/generator/execute-commissioning
        /// </summary>
        // ====================================================================
        // API 5: EXECUTE COMMISSIONING JOB
        // ====================================================================
        // Mục đích: Trigger OSS Netact chạy job commissioning cho 52 sites
        // Input:
        //   - ossJobId: "oss_job_123" (từ API 4)
        //   - commissionType: "AutoPnP"
        // Output:
        //   - executionId: "exec_456" (unique ID để track progress)
        //   - status: "running"
        //   - startTime: "2024-12-14T10:30:00Z"
        // Workflow:
        //   1. Sau khi upload XML xong (API 4)
        //   2. Gọi API này để bảo OSS Netact "bắt đầu chạy commissioning"
        //   3. OSS Netact nhận lệnh, bắt đầu apply config cho 52 sites
        //   4. OSS Netact tạo executionId để track
        //   5. Return executionId để dùng cho API 6 (polling progress)
        // Lưu ý: Job này chạy lâu (45-60 phút cho 52 sites)
        // ====================================================================
        [HttpPost("execute-commissioning")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExecuteCommissioning([FromBody] ExecuteCommissioningRequest request)
        {
            try
            {
                var result = await _ossService.TriggerCommissioningAsync(
                    request.OssJobId,
                    request.CommissionType);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error triggering commissioning job",
                    error = ex.Message
                });
            }
        }
        //========================================================================

        public class ExecuteCommissioningRequest
        {
            public string OssJobId { get; set; }
            public string CommissionType { get; set; }
        }
        //========================================================================

        /// <summary>
        /// API 6: Get Job Status (for polling progress)
        /// GET /api/pnp5Gsran-declaration/generator/status/{executionId}
        /// </summary>
        // ====================================================================
        // API 6: GET JOB STATUS (POLLING)
        // ====================================================================
        // Mục đích: Lấy tiến trình real-time của job đang chạy (gọi liên tục mỗi 2-3s)
        // Input: executionId (string) - "exec_456" (từ API 5)
        // Output:
        //   - status: "running" | "completed" | "failed"
        //   - percentage: 65% (đã xong 65%)
        //   - currentSite: "MRBTS-1600025" (đang xử lý site này)
        //   - completedSites: 34 (đã xong 34 sites)
        //   - failedSites: 2 (2 sites lỗi)
        //   - totalSites: 52
        //   - message: "Processing site 35/52..."
        // Workflow:
        //   1. Sau khi trigger job (API 5)
        //   2. Frontend gọi API này LIÊN TỤC mỗi 2 giây
        //   3. Backend query OSS Netact để lấy progress mới nhất
        //   4. Update progress bar: 0% → 10% → 20% → ... → 100%
        //   5. Khi status = "completed" → Stop polling, chuyển Step 6
        // Use case: Hiển thị progress bar real-time cho user
        // Lưu ý: API này được gọi rất nhiều lần (polling)
        // ====================================================================
        [HttpGet("status/{executionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobStatus(string executionId)
        {
            try
            {
                var status = await _ossService.GetExecutionStatusAsync(executionId);

                return Ok(new
                {
                    success = true,
                    data = status
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting job status",
                    error = ex.Message
                });
            }
        }
        //========================================================================


        /// <summary>
        /// API 7: Get Final Results
        /// GET /api/pnp5Gsran-declaration/generator/results/{executionId}
        /// </summary>
        /// 
        // ====================================================================
        // API 7: GET FINAL RESULTS
        // ====================================================================
        // Mục đích: Lấy kết quả cuối cùng khi job hoàn thành (hiển thị ở Step 6)
        // Input: executionId (string) - "exec_456"
        // Output:
        //   - status: "completed"
        //   - totalSites: 52
        //   - successfulSites: 50 (50 sites OK)
        //   - failedSites: 2 (2 sites lỗi)
        //   - duration: "45 minutes"
        //   - results: [chi tiết từng site: success/failed + error message]
        //   - downloadLinks: {
        //       xmlZip: "/api/.../download/job_xxx",
        //       errorReport: "/api/.../error-report/exec_456",
        //       executionLog: "/api/.../logs/exec_456"
        //     }
        // Workflow:
        //   1. Sau khi API 6 báo status = "completed"
        //   2. Gọi API này để lấy summary chi tiết
        //   3. Hiển thị ở Step 6:
        //      - Cards: Total/Success/Failed/Duration
        //      - Table: List sites + status
        //      - Download buttons: XML, Error Report, Logs
        // Use case: Báo cáo kết quả cho user, cho phép download files
        // ====================================================================
        [HttpGet("results/{executionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobResults(string executionId)
        {
            try
            {
                var results = await _ossService.GetExecutionResultsAsync(executionId);

                return Ok(new
                {
                    success = true,
                    data = results
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting job results",
                    error = ex.Message
                });
            }
        }




        /// <summary>
        /// Download Error Report
        /// GET /api/pnp5Gsran-declaration/generator/error-report/{executionId}
        /// </summary>
        // ====================================================================
        // BONUS API: DOWNLOAD ERROR REPORT
        // ====================================================================
        // Mục đích: Download file PDF báo cáo các sites bị lỗi
        // Input: executionId
        // Output: File PDF
        // Workflow: User click "Download Error Report" → Download PDF
        // ====================================================================

        [HttpGet("error-report/{executionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadErrorReport(string executionId)
        {
            try
            {
                var reportData = await _ossService.GetErrorReportAsync(executionId);

                return File(
                    reportData,
                    "application/pdf",
                    $"error_report_{executionId}.pdf"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error downloading error report",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Download Execution Logs
        /// GET /api/pnp5Gsran-declaration/generator/logs/{executionId}
        /// </summary>
        // ====================================================================
        // BONUS API: DOWNLOAD EXECUTION LOGS
        // ====================================================================
        // Mục đích: Download file text log chi tiết quá trình thực thi
        // Input: executionId
        // Output: File TXT
        // Workflow: User click "Download Logs" → Download TXT log file
        // ====================================================================
        [HttpGet("logs/{executionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadLogs(string executionId)
        {
            try
            {
                var logData = await _ossService.GetExecutionLogsAsync(executionId);

                return File(
                    logData,
                    "text/plain",
                    $"execution_log_{executionId}.txt"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error downloading logs",
                    error = ex.Message
                });
            }
        }



        // ====================================================================
        // NEW API: GET XML FILE CONTENT
        // ====================================================================
        // Mục đích: Đọc nội dung file XML đã generate để hiển thị trên frontend
        // Input: 
        //   - jobId: ID của job generation (VD: "job_20251217_141845")
        //   - fileName: Tên file XML (VD: "5G-CGY017M-HNI.xml")
        // Output: Nội dung XML dạng text
        // Workflow:
        //   1. User click "View" button trên bảng Generated Files
        //   2. Frontend gọi API này với jobId + fileName
        //   3. Backend tìm file trong folder: GeneratedXML/{jobId}/{fileName}
        //   4. Đọc nội dung file XML
        //   5. Return XML content dạng text
        //   6. Frontend hiển thị trong modal với syntax highlighting
        // Use case: Xem nội dung XML để kiểm tra trước khi upload lên OSS
        // Lưu ý: Chỉ đọc được file đã generate thành công, file failed không có
        // ====================================================================
        /// <summary>
        /// NEW API: Get XML File Content
        /// GET /api/pnp5Gsran-declaration/generator/xml-content/{jobId}/{fileName}
        /// </summary>
        [HttpGet("xml-content/{jobId}/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetXMLContent(string jobId, string fileName)
        {
            try
            {
                var content = await _generatorService.GetXMLFileContentAsync(jobId, fileName);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fileName = fileName,
                        content = content
                    }
                });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error reading XML file",
                    error = ex.Message
                });
            }
        }
        //========================================================================

        // ====================================================================
        // NEW API: DOWNLOAD SINGLE XML FILE
        // ====================================================================
        // Mục đích: Download 1 file XML riêng lẻ (không phải toàn bộ ZIP)
        // Input: 
        //   - jobId: ID của job generation (VD: "job_20251217_163809")
        //   - fileName: Tên file XML (VD: "5G-CGY017M-HNI.xml")
        // Output: File XML (binary download)
        // Workflow:
        //   1. User click "Download" button trên bảng Generated Files
        //   2. Frontend gọi API này với jobId + fileName
        //   3. Backend tìm file trong folder: GeneratedXML/{jobId}/{fileName}
        //   4. Đọc file dạng binary
        //   5. Return file với Content-Disposition header để browser tự động download
        //   6. Browser download file với tên gốc
        // Use case: Download từng file XML riêng lẻ để kiểm tra hoặc backup
        // Khác với API 3 (download ZIP): API này download 1 file, API 3 download tất cả
        // ====================================================================
        /// <summary>
        /// NEW API: Download Single XML File
        /// GET /api/pnp5Gsran-declaration/generator/download-file/{jobId}/{fileName}
        /// </summary>
        [HttpGet("download-file/{jobId}/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadSingleFile(string jobId, string fileName)
        {
            try
            {
                var fileData = await _generatorService.GetSingleXMLFileAsync(jobId, fileName);

                return File(
                    fileData,
                    "application/xml",
                    fileName
                );
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error downloading XML file",
                    error = ex.Message
                });
            }
        }







        #endregion
        //========================================================================



    }
    // ket thuc public class FunctProcessingApiController : ControllerBase


}
