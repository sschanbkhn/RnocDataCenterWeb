using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.PnP5GSRANDeclaration
{
    public class ImplementationGeneratorService : InterfaceGeneratorService
    {
        private readonly string _templateFolderPath;
        private readonly string _outputFolderPath;
        // private readonly string _templatePath;        // ← PHẢI CÓ
        private readonly string _connectionString;  // ← THÊM DÒNG NÀY

        private readonly IConfiguration _config;  // ← THÊM field

        // ====================================================================
        // ✅ Constructor - Tự tính path (như R003 và OSSService)
        // ====================================================================

        /*
        public ImplementationGeneratorService(string templateFolderPath, string outputFolderPath)
        {
            _templateFolderPath = templateFolderPath;
            _outputFolderPath = outputFolderPath;

            // Ensure output folder exists
            if (!Directory.Exists(_outputFolderPath))
            {
                Directory.CreateDirectory(_outputFolderPath);
            }
        }
        */

        public ImplementationGeneratorService(IConfiguration config)  // ← THÊM parameter
        {

            _config = config;  // ← THÊM
            _connectionString = _config.GetConnectionString("InformationProductionConnection");  // ← THÊM


            // Tự động tìm paths
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appFolder = Path.GetDirectoryName(assemblyPath);
            var projectRoot = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(appFolder)
                )
            );

            _templateFolderPath = Path.Combine(
                projectRoot,
                "Controllers",
                "NSN",
                "PnP5GSRANDeclaration",
                "ConfigPnP"
            );

            _outputFolderPath = Path.Combine(
                projectRoot,
                "Controllers",
                "NSN",
                "PnP5GSRANDeclaration",
                "GeneratedXML"
            );

            /// Tạo folders nếu chưa có
            if (!Directory.Exists(_templateFolderPath)) // ← SỬA
            {
                Directory.CreateDirectory(_templateFolderPath); // ← SỬA
            }

            if (!Directory.Exists(_outputFolderPath))
            {
                Directory.CreateDirectory(_outputFolderPath);
            }

        }




        // ====================================================================
        // Get Template Content
        // ====================================================================

        public async Task<string> GetTemplateContentAsync(string commissionType, string fileName)
        {
            var filePath = Path.Combine(_templateFolderPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Template file not found: {fileName}");
            }

            return await File.ReadAllTextAsync(filePath);
        }

        // ====================================================================
        // Generate XML Files
        // ====================================================================
        public async Task<GenerateXMLResponse> GenerateXMLFilesAsync(GenerateXMLRequest request)
        {
            Console.WriteLine("🔵 Step A: Enter GenerateXMLFilesAsync");
            Console.WriteLine($"🔵 CommissionType: {request.CommissionType}");
            Console.WriteLine($"🔵 Sites count: {request.Sites.Count}");

            var jobId = $"job_{DateTime.Now:yyyyMMdd_HHmmss}";
            var jobFolderPath = Path.Combine(_outputFolderPath, jobId);
            Directory.CreateDirectory(jobFolderPath);

            var generatedFiles = new List<GeneratedFileResult>();

            Console.WriteLine($"🔵 Output path: {_outputFolderPath}");
            Console.WriteLine($"🔵 Job folder: {jobFolderPath}");


            foreach (var site in request.Sites)
            {
                try
                {
                    // Find template file
                    var templateFileName = await FindTemplateFileNameAsync(site.TemplateId);

                    if (string.IsNullOrEmpty(templateFileName))
                    {
                        generatedFiles.Add(new GeneratedFileResult
                        {
                            SiteId = site.Id,
                            FileName = "",
                            Status = "failed",
                            Error = "Template not found"
                        });
                        continue;
                    }

                    // Load template content
                    var templateContent = await GetTemplateContentAsync(request.CommissionType, templateFileName);

                    // Replace placeholders with site data
                    var xmlContent = ReplacePlaceholders(templateContent, site);

                    // Save generated XML
                    var outputFileName = $"{site.SiteName}.xml";
                    var outputFilePath = Path.Combine(jobFolderPath, outputFileName);
                    await File.WriteAllTextAsync(outputFilePath, xmlContent);

                    Console.WriteLine($"✅ File written successfully");




                    generatedFiles.Add(new GeneratedFileResult
                    {
                        SiteId = site.Id,
                        FileName = outputFileName,
                        Status = "success",
                        Error = null
                    });
                }
                catch (Exception ex)
                {
                    generatedFiles.Add(new GeneratedFileResult
                    {
                        SiteId = site.Id,
                        FileName = "",
                        Status = "failed",
                        Error = ex.Message
                    });
                }
            }

            return new GenerateXMLResponse
            {
                JobId = jobId,
                TotalSites = request.Sites.Count,
                GeneratedFiles = generatedFiles
            };
        }

        // ====================================================================
        // Get XML ZIP
        // ====================================================================
        public async Task<byte[]> GetXMLZipAsync(string jobId)
        {
            var jobFolderPath = Path.Combine(_outputFolderPath, jobId);

            if (!Directory.Exists(jobFolderPath))
            {
                throw new DirectoryNotFoundException($"Job folder not found: {jobId}");
            }

            var zipFilePath = Path.Combine(_outputFolderPath, $"{jobId}.zip");

            // Create ZIP file
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(jobFolderPath, zipFilePath);

            // Read ZIP bytes
            var zipBytes = await File.ReadAllBytesAsync(zipFilePath);

            return zipBytes;
        }

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        private async Task<string> FindTemplateFileNameAsync(string templateId)
        {
            // Find template file by ID (filename without extension)
            var files = Directory.GetFiles(_templateFolderPath, "*.xml");
            var templateFile = files.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f) == templateId);

            return templateFile != null ? Path.GetFileName(templateFile) : null;
        }

        private string ReplacePlaceholders(string templateContent, SiteDataDTO site)
        {
            // Replace placeholders in XML template
            // Format: {{FIELD_NAME}} or ${FIELD_NAME}

            var result = templateContent;

            // Replace common fields
            result = result.Replace("{{SITE_NAME}}", site.SiteName ?? "");
            result = result.Replace("{{IP_OAM}}", site.IpOam ?? "");
            result = result.Replace("{{BAND_5G}}", site.Band5g ?? "");
            result = result.Replace("{{RRH_5G}}", site.Rrh5g ?? "");

            // Alternative format with ${}
            result = result.Replace("${SITE_NAME}", site.SiteName ?? "");
            result = result.Replace("${IP_OAM}", site.IpOam ?? "");
            result = result.Replace("${BAND_5G}", site.Band5g ?? "");
            result = result.Replace("${RRH_5G}", site.Rrh5g ?? "");

            // TODO: Add more field replacements as needed

            return result;
        }
        // ====================================================================

        /// <summary>
        /// Đọc nội dung file XML từ folder GeneratedXML/{jobId}/{fileName}
        /// Throw FileNotFoundException nếu file không tồn tại
        /// </summary>
        public async Task<string> GetXMLFileContentAsync(string jobId, string fileName)
        {
            // Tìm file trong folder job
            var jobFolderPath = Path.Combine(_outputFolderPath, jobId);
            var filePath = Path.Combine(jobFolderPath, fileName);

            // Check file tồn tại
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"XML file not found: {fileName} in job {jobId}");
            }

            // Đọc toàn bộ nội dung file
            var content = await File.ReadAllTextAsync(filePath);
            return content;
        }


        /// <summary>
        /// Đọc file XML dạng binary để download
        /// Khác với GetXMLFileContentAsync (đọc text), method này đọc binary
        /// </summary>
        public async Task<byte[]> GetSingleXMLFileAsync(string jobId, string fileName)
        {
            // Tìm file trong folder job
            var jobFolderPath = Path.Combine(_outputFolderPath, jobId);
            var filePath = Path.Combine(jobFolderPath, fileName);

            // Check file tồn tại
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"XML file not found: {fileName} in job {jobId}");
            }

            // Đọc toàn bộ file dạng binary
            var fileData = await File.ReadAllBytesAsync(filePath);
            return fileData;
        }


        // ========== NEW - DATABASE METHODS ==========

        /// <summary>
        /// Purpose: Save new job to database
        /// Input: JobData (job info)
        /// Output: job_id (BIGINT)
        /// Workflow:
        ///   1. Open DB connection
        ///   2. Generate job_number (J0001, J0002...)
        ///   3. INSERT into objtablesran5gpnpjobs
        ///   4. Return job_id
        ///   /// <summary>
        /// Mục đích: Lưu job mới vào database
        /// Input: Thông tin job (loại, file, số lượng sites)
        /// Output: job_id (BIGINT)
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Tạo job_number tự động (J0001, J0002...)
        ///   3. INSERT vào objtablesran5gpnpjobs
        ///   4. Trả về job_id
        /// </summary>
        /// </summary>
        public async Task<long> SaveJobAsync(
            string jobType,           // "SiteConfig" hoặc "AutoPnP"
            string inputFileName,     // Tên file Excel
            string inputFilePath,     // Đường dẫn file
            int totalSites,           // Tổng số sites từ Excel
            string createdBy = "admin"
        )
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Generate job_number (J0001, J0002...)
                var jobNumberQuery = @"
                SELECT COALESCE(MAX(CAST(SUBSTRING(job_number FROM 2) AS INTEGER)), 0) + 1
                FROM system_nsn_declare5gzerotouch.objtablesran5gpnpjobs";

                int nextNumber;
                using (var cmd = new NpgsqlCommand(jobNumberQuery, connection))
                {
                    nextNumber = (int)await cmd.ExecuteScalarAsync();
                }

                var jobNumber = $"J{nextNumber:D4}"; // J0001, J0002...

                // BƯỚC 2: INSERT job
                var insertQuery = @"
                INSERT INTO system_nsn_declare5gzerotouch.objtablesran5gpnpjobs
                (job_number, job_type, status, input_file_name, input_file_path, 
                 total_sites, created_by, created_at, started_at)
                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)
                RETURNING job_id";

                using (var cmd = new NpgsqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("p1", jobNumber);
                    cmd.Parameters.AddWithValue("p2", jobType);
                    cmd.Parameters.AddWithValue("p3", "Processing"); // Status = Processing
                    cmd.Parameters.AddWithValue("p4", inputFileName);
                    cmd.Parameters.AddWithValue("p5", inputFilePath);
                    cmd.Parameters.AddWithValue("p6", totalSites);
                    cmd.Parameters.AddWithValue("p7", createdBy);
                    cmd.Parameters.AddWithValue("p8", DateTime.Now); // created_at
                    cmd.Parameters.AddWithValue("p9", DateTime.Now); // started_at

                    var jobId = (long)await cmd.ExecuteScalarAsync();

                    Console.WriteLine($"✅ Created job: {jobNumber} (ID: {jobId})");

                    return jobId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveJobAsync failed: {ex.Message}", ex);
            }
        }
        // ket thuc private async Task<long> SaveJobAsync(


        // ====================================================================
        // Lưu danh sách sites vào validation table (bulk insert)
        // ====================================================================
        /// <summary>
        /// Mục đích: Bulk insert tất cả sites từ Excel vào validation table
        /// Input: job_id, danh sách sites (62 columns)
        /// Output: void
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Sử dụng COPY command để bulk insert
        ///   3. Insert từng site với 62 columns
        /// </summary>
        public async Task SaveValidationSitesAsync(
            long jobId,
            List<SiteValidationData> sites
        )
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Bulk insert bằng COPY command
                using var writer = connection.BeginBinaryImport(@"
            COPY system_nsn_declare5gzerotouch.objtablesran5gpnpsites_validation 
            (job_id, site_order, validation_status, validation_errors, 
             province_code, district_code,
             stt, mrbts_name, address, oam, men, tac, province, district,
             serial_sm_5g, nrbts_id_5g, mrbts_id, mrbts_name_5g, mrbts_name_4g,
             vlan_ms_plane_5g, ip_ms_plane_5g, gateway_ip_ms_plane_5g,
             vlan_cu_plane_5g, ip_cu_plane_5g, gateway_ip_cu_plane_5g,
             ip_ms_plane_4g, ip_cu_plane_4g,
             netact_oms, primary_dns, secondary_dns, dcap_ip, netact_oam,
             phys_cell_id_1, phys_cell_id_2, phys_cell_id_3,
             root_seq_index_1, root_seq_index_2, root_seq_index_3,
             beamset_1, beamset_2, beamset_3,
             tilt_offset_1, tilt_offset_2, tilt_offset_3,
             profile, fbb_manager, management_cell_5g,
             band_5g, band_4g, sm, rrh_5g, rrh_4g, port_information, nrarfcn, channel_bandwidth,
             rmod_5g, rmod_4g, antl_2g, antl_3g, antl_4g)
            FROM STDIN (FORMAT BINARY)
        ");

                // BƯỚC 2: Write từng site
                foreach (var site in sites)
                {
                    writer.StartRow();

                    // System columns (6)
                    writer.Write(jobId, NpgsqlDbType.Bigint);
                    writer.Write(site.SiteOrder, NpgsqlDbType.Integer);
                    writer.Write(site.ValidationStatus ?? "Valid", NpgsqlDbType.Varchar);
                    writer.Write(site.ValidationErrors ?? (object)DBNull.Value, NpgsqlDbType.Text);

                    // Mapping columns (2)
                    writer.Write(site.ProvinceCode ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.DistrictCode ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Excel columns - Group 1: Identification (8)
                    writer.Write(site.Stt ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.MrbtsName ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Address ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Oam ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Men ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Tac ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Province ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.District ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 2: Serial & IDs (5)
                    writer.Write(site.SerialSm5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.NrbtsId5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.MrbtsId ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.MrbtsName5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.MrbtsName4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 3: 5G Network Config (6)
                    writer.Write(site.VlanMsPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.IpMsPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.GatewayIpMsPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.VlanCuPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.IpCuPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.GatewayIpCuPlane5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 4: 4G Network Config (2)
                    writer.Write(site.IpMsPlane4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.IpCuPlane4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 5: DNS & Network (5)
                    writer.Write(site.NetactOms ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.PrimaryDns ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.SecondaryDns ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.DcapIp ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.NetactOam ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 6: Cell Configuration (15)
                    writer.Write(site.PhysCellId1 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.PhysCellId2 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.PhysCellId3 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.RootSeqIndex1 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.RootSeqIndex2 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.RootSeqIndex3 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.Beamset1 ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Beamset2 ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Beamset3 ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.TiltOffset1 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.TiltOffset2 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.TiltOffset3 ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(site.Profile ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.FbbManager ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.ManagementCell5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 7: Band & Equipment (8)
                    writer.Write(site.Band5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Band4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Sm ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Rrh5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Rrh4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.PortInformation ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Nrarfcn ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.ChannelBandwidth ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    // Group 8: RMOD & ANTL (5)
                    writer.Write(site.Rmod5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Rmod4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Antl2g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Antl3g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(site.Antl4g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                }
                // ket thuc foreach (var site in sites)
                // ====================================================================

                await writer.CompleteAsync();

                Console.WriteLine($"✅ Đã lưu {sites.Count} sites vào validation table");
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveValidationSitesAsync thất bại: {ex.Message}", ex);
            }
            // ket thuc try catch
            // ====================================================================
        }
        // ket thuc SaveValidationSitesAsync
        // ====================================================================




        // ====================================================================
        // Lưu kết quả template matching (bulk insert)
        // ====================================================================
        /// <summary>
        /// Mục đích: Bulk insert kết quả template matching cho các sites
        /// Input: job_id, danh sách template matching (23 columns)
        /// Output: void
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Sử dụng COPY command để bulk insert
        ///   3. Insert từng site với matching results + site info
        /// </summary>
        public async Task SaveTemplateMatchingAsync(
            long jobId,
            List<SiteTemplateData> templates
        )
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Bulk insert bằng COPY command
                using var writer = connection.BeginBinaryImport(@"
            COPY system_nsn_declare5gzerotouch.objtablesran5gpnpsites_templates 
            (job_id, validation_id, 
             match_status, matched_template_name, template_file_path, match_method, match_reason,
             stt, mrbts_name, address, ip_oam, province, district,
             serial_sm_5g, nrbts_id_5g, mrbts_id, mrbts_name_5g,
             profile, fbb_manager, management_cell_5g, sm, band_5g, rrh_5g, channel_bandwidth)
            FROM STDIN (FORMAT BINARY)
        ");

                // BƯỚC 2: Write từng template matching record
                foreach (var template in templates)
                {
                    writer.StartRow();

                    // System columns (2)
                    writer.Write(jobId, NpgsqlDbType.Bigint);
                    writer.Write(template.ValidationId, NpgsqlDbType.Bigint);

                    // Matching results (5)
                    writer.Write(template.MatchStatus ?? "Matched", NpgsqlDbType.Varchar);
                    writer.Write(template.MatchedTemplateName ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.TemplateFilePath ?? (object)DBNull.Value, NpgsqlDbType.Text);
                    writer.Write(template.MatchMethod ?? "Auto", NpgsqlDbType.Varchar);
                    writer.Write(template.MatchReason ?? (object)DBNull.Value, NpgsqlDbType.Text);

                    // Site info duplicated (17 columns)
                    writer.Write(template.Stt ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                    writer.Write(template.MrbtsName ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.Address ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.IpOam ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.Province ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.District ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    writer.Write(template.SerialSm5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.NrbtsId5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.MrbtsId ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.MrbtsName5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);

                    writer.Write(template.Profile ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.FbbManager ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.ManagementCell5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.Sm ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.Band5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.Rrh5g ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(template.ChannelBandwidth ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                }
                // ket thuc foreach (var template in templates)
                // ====================================================================

                await writer.CompleteAsync();

                Console.WriteLine($"✅ Đã lưu {templates.Count} template matching records");
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveTemplateMatchingAsync thất bại: {ex.Message}", ex);
            }
            // ket thuc try catch
            // ====================================================================
        }
        // ket thuc SaveTemplateMatchingAsync
        // ====================================================================


        // ====================================================================
        // Lưu kết quả XML generation (support retry)
        // ====================================================================
        /// <summary>
        /// Mục đích: Lưu kết quả XML generation cho các sites (support retry)
        /// Input: job_id, danh sách generation results
        /// Output: void
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Sử dụng COPY command để bulk insert
        ///   3. Insert từng generation result (có thể nhiều attempts cho 1 site)
        /// </summary>
        public async Task SaveGenerationResultsAsync(
            long jobId,
            List<SiteGenerationData> generations
        )
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Bulk insert bằng COPY command
                using var writer = connection.BeginBinaryImport(@"
            COPY system_nsn_declare5gzerotouch.objtablesran5gpnpsites_generation 
            (job_id, validation_id, template_id, attempt_number,
             generation_status, xml_file_name, xml_file_path, error_message,
             generated_at, duration_seconds)
            FROM STDIN (FORMAT BINARY)
        ");

                // BƯỚC 2: Write từng generation result
                foreach (var gen in generations)
                {
                    writer.StartRow();

                    // Foreign keys
                    writer.Write(jobId, NpgsqlDbType.Bigint);
                    writer.Write(gen.ValidationId, NpgsqlDbType.Bigint);
                    writer.Write(gen.TemplateId, NpgsqlDbType.Bigint);

                    // Retry support
                    writer.Write(gen.AttemptNumber, NpgsqlDbType.Integer);

                    // Generation results
                    writer.Write(gen.GenerationStatus ?? "Success", NpgsqlDbType.Varchar);
                    writer.Write(gen.XmlFileName ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                    writer.Write(gen.XmlFilePath ?? (object)DBNull.Value, NpgsqlDbType.Text);
                    writer.Write(gen.ErrorMessage ?? (object)DBNull.Value, NpgsqlDbType.Text);

                    // Timestamps
                    writer.Write(gen.GeneratedAt, NpgsqlDbType.Timestamp);
                    writer.Write(gen.DurationSeconds ?? (object)DBNull.Value, NpgsqlDbType.Integer);
                }
                // ket thuc foreach (var gen in generations)
                // ====================================================================

                await writer.CompleteAsync();

                Console.WriteLine($"✅ Đã lưu {generations.Count} generation results");
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveGenerationResultsAsync thất bại: {ex.Message}", ex);
            }
            // ket thuc try catch
            // ====================================================================
        }
        // ket thuc SaveGenerationResultsAsync
        // ====================================================================


        // ====================================================================
        // Lưu kết quả OSS execution (support retry)
        // ====================================================================
        /// <summary>
        /// Mục đích: Lưu kết quả OSS execution cho các sites (support retry)
        /// Input: job_id, danh sách OSS execution results
        /// Output: void
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Sử dụng COPY command để bulk insert
        ///   3. Insert từng execution result (có thể nhiều attempts cho 1 site)
        /// </summary>
        public async Task SaveOssExecutionAsync(long jobId, List<SiteOssExecutionData> executions)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Bulk insert bằng COPY command
                using var writer = connection.BeginBinaryImport(@"
            COPY system_nsn_declare5gzerotouch.objtablesran5gpnposs_executions 
            (job_id, generation_id, attempt_number,
             oss_file_path, oss_command, oss_response,
             upload_status, execution_status,
             uploaded_at, executed_at, completed_at, duration_seconds,
             error_message)
            FROM STDIN (FORMAT BINARY)
        ");

                // BƯỚC 2: Write từng execution result
                foreach (var exec in executions)
                {
                    writer.StartRow();

                    // Foreign keys
                    writer.Write(jobId, NpgsqlDbType.Bigint);
                    writer.Write(exec.GenerationId, NpgsqlDbType.Bigint);

                    // Retry support
                    writer.Write(exec.AttemptNumber, NpgsqlDbType.Integer);

                    // OSS execution details
                    writer.Write(exec.OssFilePath ?? (object)DBNull.Value, NpgsqlDbType.Text);
                    writer.Write(exec.OssCommand ?? (object)DBNull.Value, NpgsqlDbType.Text);
                    writer.Write(exec.OssResponse ?? (object)DBNull.Value, NpgsqlDbType.Text);

                    // Status
                    writer.Write(exec.UploadStatus ?? "Pending", NpgsqlDbType.Varchar);
                    writer.Write(exec.ExecutionStatus ?? "Pending", NpgsqlDbType.Varchar);

                    // Timestamps
                    writer.Write(exec.UploadedAt ?? (object)DBNull.Value, NpgsqlDbType.Timestamp);
                    writer.Write(exec.ExecutedAt ?? (object)DBNull.Value, NpgsqlDbType.Timestamp);
                    writer.Write(exec.CompletedAt ?? (object)DBNull.Value, NpgsqlDbType.Timestamp);
                    writer.Write(exec.DurationSeconds ?? (object)DBNull.Value, NpgsqlDbType.Integer);

                    // Error
                    writer.Write(exec.ErrorMessage ?? (object)DBNull.Value, NpgsqlDbType.Text);
                }
                // ket thuc foreach (var exec in executions)
                // ====================================================================

                await writer.CompleteAsync();

                Console.WriteLine($"✅ Đã lưu {executions.Count} OSS execution results");
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveOssExecutionAsync thất bại: {ex.Message}", ex);
            }
            // ket thuc try catch
            // ====================================================================
        }
        // ket thuc SaveOssExecutionAsync
        // ====================================================================


        // ====================================================================
        // Cập nhật thống kê job
        // ====================================================================
        /// <summary>
        /// Mục đích: Cập nhật thống kê job sau khi hoàn thành các bước
        /// Input: job_id, thống kê (validation, matching, generation, OSS)
        /// Output: void
        /// Workflow:
        ///   1. Mở kết nối DB
        ///   2. Build UPDATE query với các field không null
        ///   3. Execute UPDATE
        ///   4. Tính duration_seconds nếu completed_at được set
        /// </summary>
        public async Task UpdateJobStatisticsAsync(long jobId, UpdateJobStatisticsData statistics)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // BƯỚC 1: Build UPDATE query động (chỉ update fields không null)
                var updateFields = new List<string>();
                var parameters = new List<NpgsqlParameter>();
                var paramIndex = 1;

                if (statistics.TotalSites.HasValue)
                {
                    updateFields.Add($"total_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.TotalSites.Value));
                    paramIndex++;
                }

                if (statistics.ValidSites.HasValue)
                {
                    updateFields.Add($"valid_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.ValidSites.Value));
                    paramIndex++;
                }

                if (statistics.InvalidSites.HasValue)
                {
                    updateFields.Add($"invalid_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.InvalidSites.Value));
                    paramIndex++;
                }

                if (statistics.MatchedSites.HasValue)
                {
                    updateFields.Add($"matched_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.MatchedSites.Value));
                    paramIndex++;
                }

                if (statistics.UnmatchedSites.HasValue)
                {
                    updateFields.Add($"unmatched_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.UnmatchedSites.Value));
                    paramIndex++;
                }

                if (statistics.GeneratedSuccessSites.HasValue)
                {
                    updateFields.Add($"generated_success_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.GeneratedSuccessSites.Value));
                    paramIndex++;
                }

                if (statistics.GeneratedFailedSites.HasValue)
                {
                    updateFields.Add($"generated_failed_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.GeneratedFailedSites.Value));
                    paramIndex++;
                }

                if (statistics.OssSuccessSites.HasValue)
                {
                    updateFields.Add($"oss_success_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.OssSuccessSites.Value));
                    paramIndex++;
                }

                if (statistics.OssFailedSites.HasValue)
                {
                    updateFields.Add($"oss_failed_sites = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.OssFailedSites.Value));
                    paramIndex++;
                }

                if (!string.IsNullOrEmpty(statistics.Status))
                {
                    updateFields.Add($"status = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.Status));
                    paramIndex++;
                }

                if (statistics.CompletedAt.HasValue)
                {
                    updateFields.Add($"completed_at = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.CompletedAt.Value));
                    paramIndex++;
                }

                if (statistics.DurationSeconds.HasValue)
                {
                    updateFields.Add($"duration_seconds = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.DurationSeconds.Value));
                    paramIndex++;
                }

                if (!string.IsNullOrEmpty(statistics.ErrorMessage))
                {
                    updateFields.Add($"error_message = @p{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"p{paramIndex}", statistics.ErrorMessage));
                    paramIndex++;
                }

                // Nếu không có field nào để update
                if (updateFields.Count == 0)
                {
                    Console.WriteLine("⚠️ Không có field nào để update");
                    return;
                }
                // ket thuc if (updateFields.Count == 0)
                // ====================================================================

                // BƯỚC 2: Build và execute UPDATE query
                var updateQuery = $@"
            UPDATE system_nsn_declare5gzerotouch.objtablesran5gpnpjobs
            SET {string.Join(", ", updateFields)}
            WHERE job_id = @jobId";

                using (var cmd = new NpgsqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.Parameters.AddWithValue("jobId", jobId);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine($"✅ Đã cập nhật job {jobId} - {rowsAffected} rows affected");
                }
                // ket thuc using cmd
                // ====================================================================
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateJobStatisticsAsync thất bại: {ex.Message}", ex);
            }
            // ket thuc try catch
            // ====================================================================
        }
        // ket thuc UpdateJobStatisticsAsync
        // ====================================================================


    }
    // ket thuc public class ImplementationGeneratorService : InterfaceGeneratorService
    // ====================================================================

}
