using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.PnP5GSRANDeclaration
{
    public class ImplementationGeneratorService : InterfaceGeneratorService
    {
        private readonly string _templateFolderPath;
        private readonly string _outputFolderPath;
        // private readonly string _templatePath;        // ← PHẢI CÓ

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

        public ImplementationGeneratorService()
        {
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


    }
    // ket thuc public class ImplementationGeneratorService : InterfaceGeneratorService
    // ====================================================================

}
