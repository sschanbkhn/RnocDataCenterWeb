using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.PnP5GSRANDeclaration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.PnP5GSRANDeclaration
{
    public class ImplementationOSSService : InterfaceOSSService
    {
        private readonly string _outputFolderPath;
        private readonly string _ossNetactUrl;
        private readonly HttpClient _httpClient;

        // SFTP credentials
        private readonly string _sftpHost = "10.149.186.20"; // ← TODO: Anh cho IP/hostname
        private readonly int _sftpPort = 22;
        private readonly string _sftpUsername = "bvthem";
        private readonly string _sftpPassword = "Bk123456-";
        private readonly string _sftpBasePath = "/d/oss/global/var/pm/shared/content3/scheduler/exportCustom/Schan/CDS/SRANZeroTouch5G4G/GeneratedXML/";


        // ✅ SỬA: Constructor chỉ nhận outputPath
        // public ImplementationOSSService(string outputFolderPath)
        public ImplementationOSSService() // ← BỎ string outputFolderPath
        {
            // _outputFolderPath = outputFolderPath;
            // Tự động tìm path GeneratedXML
            /*
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appFolder = Path.GetDirectoryName(assemblyPath);
            var projectRoot = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(appFolder)
                )
            );

            _outputFolderPath = Path.Combine(
                projectRoot,
                "Controllers",
                "NSN",
                "PnP5GSRANDeclaration",
                "GeneratedXML"
            );
            */

            // ✅ AUTO-DETECT: Thử 2 đường dẫn, cái nào tồn tại thì dùng

            // Path 1: Cùng cấp với DLL (Ubuntu Server)
            var basePath = Directory.GetCurrentDirectory();
            var path1Output = Path.Combine(basePath, "GeneratedXML");

            // Path 2: Trong Controllers (Local Development)
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var appFolder = Path.GetDirectoryName(assemblyPath);

            string path2Output = null;
            try
            {
                var projectRoot = Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(appFolder)
                    )
                );

                if (!string.IsNullOrEmpty(projectRoot))
                {
                    path2Output = Path.Combine(projectRoot, "Controllers", "NSN", "PnP5GSRANDeclaration", "GeneratedXML");
                }
            }
            catch
            {
                // Ignore - path2 will be null
            }

            // Chọn output folder tồn tại
            if (Directory.Exists(path1Output))
            {
                _outputFolderPath = path1Output;
                Console.WriteLine($"✅ OSS Service using Server path: {path1Output}");
            }
            else if (path2Output != null && Directory.Exists(path2Output))
            {
                _outputFolderPath = path2Output;
                Console.WriteLine($"✅ OSS Service using Local path: {path2Output}");
            }
            else
            {
                // Không tìm thấy cả 2 → Tạo path1 (server default)
                _outputFolderPath = path1Output;
                Directory.CreateDirectory(_outputFolderPath);
                Console.WriteLine($"✅ OSS Service created output path: {path1Output}");
            }




        }


        // In-memory storage for execution tracking (replace with database in production)
        private static Dictionary<string, ExecutionStatusResponse> _executionCache = new();


        /*
        public ImplementationOSSService(
            string outputFolderPath,
            string ossNetactUrl,
            HttpClient httpClient)
        {
            _outputFolderPath = outputFolderPath;
            _ossNetactUrl = ossNetactUrl;
            _httpClient = httpClient;
        }

        */

        // ====================================================================
        // Upload XML Files to OSS Netact
        // ====================================================================
        public async Task<UploadToOSSResponse> UploadXMLFilesAsync(string jobId, string commissionType)
        {
            var jobFolderPath = Path.Combine(_outputFolderPath, jobId);

            if (!Directory.Exists(jobFolderPath))
            {
                throw new DirectoryNotFoundException($"Job folder not found: {jobId}");
            }
            // ====================================================================

            var xmlFiles = Directory.GetFiles(jobFolderPath, "*.xml");
            int uploadedFiles = 0;
            int failedFiles = 0;
            // ====================================================================

            // OSS path - Windows mapped drive
            // var ossUploadPath = @"D:\oss\global\var\pm\shared\content3\scheduler\exportCustom\Schan\CDS\SRANZeroTouch5G4G\GeneratedXML";

            // Tạo folder cho job trong OSS
            // var ossJobFolder = Path.Combine(ossUploadPath, jobId);
            // Path.Combine tự động xử lý / hoặc \
            // var ossJobFolder = Path.Combine(ossUploadPath, jobId);
            // Directory.CreateDirectory(ossJobFolder);

            // var xmlFiles = Directory.GetFiles(jobFolderPath, "*.xml");
            // var ossJobId = $"oss_{jobId}_{Guid.NewGuid():N}";
            // ====================================================================

            // int uploadedFiles = 0;
            // int failedFiles = 0;

            try
            {
                // Connect to SFTP
                using (var sftp = new SftpClient(_sftpHost, _sftpPort, _sftpUsername, _sftpPassword))
                {
                    sftp.Connect();
                    Console.WriteLine("✅ Connected to SFTP server");
                    // ====================================================================

                    // Create job folder on SFTP
                    var remoteJobFolder = $"{_sftpBasePath}/{jobId}";

                    if (!sftp.Exists(remoteJobFolder))
                    {
                        sftp.CreateDirectory(remoteJobFolder);
                        Console.WriteLine($"✅ Created remote folder: {remoteJobFolder}");
                    }
                    // ====================================================================

                    foreach (var xmlFile in xmlFiles)
                    {
                        try
                        {
                            // TODO: Call OSS Netact API to upload file
                            // var content = new MultipartFormDataContent();
                            // content.Add(new StreamContent(File.OpenRead(xmlFile)), "file", Path.GetFileName(xmlFile));
                            // var response = await _httpClient.PostAsync($"{_ossNetactUrl}/upload", content);

                            var fileName = Path.GetFileName(xmlFile);
                            var remoteFilePath = $"{remoteJobFolder}/{fileName}";

                            using (var fileStream = File.OpenRead(xmlFile))
                            {


                                sftp.UploadFile(fileStream, remoteFilePath); // ← Bỏ overwrite: true
                                Console.WriteLine($"✅ Uploaded: {fileName}");
                            }

                            uploadedFiles++;
                        }
                        // ====================================================================

                        catch (Exception ex) // ← THÊM tên biến
                        {
                            Console.WriteLine($"❌ Failed to upload {Path.GetFileName(xmlFile)}: {ex.Message}");
                            failedFiles++;
                        }
                        // ====================================================================

                    }
                    // ket thuc foreach (var xmlFile in xmlFiles)
                    // ====================================================================

                    sftp.Disconnect();
                    Console.WriteLine("✅ Disconnected from SFTP server");
                    // ====================================================================

                }
                // ket thuc using (var sftp = new SftpClient(_sftpHost, _sftpPort, _sftpUsername, _sftpPassword))
                // ====================================================================

            }
            // ====================================================================

            catch (Exception ex)
            {
                Console.WriteLine($"❌ SFTP Error: {ex.Message}");
                throw new Exception($"Failed to connect to SFTP server: {ex.Message}");
            }
            // ket thuc catch (Exception ex)
            // ====================================================================

            var ossJobId = $"oss_{jobId}_{Guid.NewGuid():N}";

            return new UploadToOSSResponse
            {
                UploadedFiles = uploadedFiles,
                FailedFiles = failedFiles,
                OssJobId = ossJobId
            };
            // return new UploadToOSSResponse
            // ====================================================================

        }
        // ket thuc public async Task<UploadToOSSResponse> UploadXMLFilesAsync(string jobId, string commissionType)

        // ====================================================================
        // Trigger Commissioning Job
        // ====================================================================
        public async Task<ExecuteCommissioningResponse> TriggerCommissioningAsync(
            string ossJobId,
            string commissionType)
        {
            var executionId = $"exec_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

            // TODO: Call OSS Netact API to trigger job
            // var requestBody = new { ossJobId, commissionType };
            // var response = await _httpClient.PostAsync($"{_ossNetactUrl}/execute", ...);

            // Extract jobId from ossJobId (format: oss_job_XXX_guid)
            // ✅ FIX: Extract jobId correctly
            // Format: oss_job_20251218_134943_guid
            // Split: [oss, job, 20251218, 134943, guid]
            var parts = ossJobId.Split('_');
            var jobId = $"{parts[1]}_{parts[2]}_{parts[3]}"; // job_20251218_134943


            // ✅ FIX: Remove trailing slash from base path
            var basePath = _sftpBasePath.TrimEnd('/');
            var remoteJobFolder = $"{basePath}/{jobId}";

            // var jobId = ossJobId.Split('_')[1] + "_" + ossJobId.Split('_')[2]; // job_20251218_134943
            // var remoteJobFolder = $"{_sftpBasePath}/{jobId}";

            Console.WriteLine($"🔵 Starting execution for jobId: {jobId}");
            Console.WriteLine($"🔵 Remote folder: {remoteJobFolder}");

            // Get list of XML files from SFTP
            List<string> xmlFiles;
            try
            {
                using (var sftp = new SftpClient(_sftpHost, _sftpPort, _sftpUsername, _sftpPassword))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteJobFolder);
                    xmlFiles = files
                        .Where(f => f.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        .Select(f => f.FullName)
                        .ToList();
                    sftp.Disconnect();
                }

                Console.WriteLine($"🔵 Found {xmlFiles.Count} XML files to execute");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to list files: {ex.Message}");
                throw new Exception($"Failed to list XML files: {ex.Message}");
            }

            // Initialize execution tracking
            _executionCache[executionId] = new ExecutionStatusResponse
            {
                ExecutionId = executionId,
                Status = "running",
                StartedAt = DateTime.UtcNow, // ← THÊM
                Progress = new ExecutionProgress
                {
                    Percentage = 0,
                    CurrentSite = "Starting...",
                    CompletedSites = 0,
                    FailedSites = 0,
                    TotalSites = xmlFiles.Count
                },
                Message = $"Executing {xmlFiles.Count} sites...",
                SiteResults = new List<SiteExecutionResult>() // ← THÊM

            };

            // Start background task to execute files
            _ = Task.Run(async () =>
            {
                try
                {
                    await ExecuteFilesInBatchesAsync(executionId, xmlFiles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Background execution error: {ex.Message}");
                    _executionCache[executionId].Status = "failed";
                    _executionCache[executionId].Message = $"Execution failed: {ex.Message}";
                }
            });

            /*
            return new ExecuteCommissioningResponse
            {
                ExecutionId = executionId,
                Status = "running",
                StartTime = DateTime.UtcNow
            };

            */
            return new ExecuteCommissioningResponse
            {
                ExecutionId = executionId,
                Status = "running",
                StartTime = DateTime.UtcNow
            };
        }

        // ====================================================================
        // Get Execution Status (Polling)
        // ====================================================================
        public async Task<ExecutionStatusResponse> GetExecutionStatusAsync(string executionId)
        {
            // TODO: Call OSS Netact API to get status
            // var response = await _httpClient.GetAsync($"{_ossNetactUrl}/status/{executionId}");

            // Return cached status (replace with real API call)
            if (_executionCache.TryGetValue(executionId, out var status))
            {
                return status;
            }

            throw new KeyNotFoundException($"Execution not found: {executionId}");
        }

        // ====================================================================
        // Get Execution Results
        // ====================================================================
        public async Task<ExecutionResultsResponse> GetExecutionResultsAsync(string executionId)
        {
            // TODO: Call OSS Netact API to get results
            // var response = await _httpClient.GetAsync($"{_ossNetactUrl}/results/{executionId}");


            // ✅ Lấy từ cache thay vì mock
            if (_executionCache.TryGetValue(executionId, out var status))
            {
                
                return new ExecutionResultsResponse
                {
                    ExecutionId = executionId,
                    Status = status.Status,
                    TotalSites = status.Progress.TotalSites,
                    SuccessfulSites = status.Progress.CompletedSites,
                    FailedSites = status.Progress.FailedSites,
                    Duration = "N/A",
                    
                    Results = status.SiteResults, // ← Dùng data thật!
                    DownloadLinks = new DownloadLinks
                    {
                        XmlZip = $"/api/pnp5Gsran-declaration/generator/download/{executionId}",
                        ErrorReport = $"/api/pnp5Gsran-declaration/generator/error-report/{executionId}",
                        ExecutionLog = $"/api/pnp5Gsran-declaration/generator/logs/{executionId}"
                    }
                };
            }
            // ====================================================================

            throw new KeyNotFoundException($"Execution not found: {executionId}");
            // ====================================================================

        }
        // ====================================================================

        // ====================================================================
        // Get Error Report
        // ====================================================================
        public async Task<byte[]> GetErrorReportAsync(string executionId)
        {
            // TODO: Generate or fetch error report from OSS Netact

            // Mock PDF report
            var reportContent = $"Error Report for Execution: {executionId}\n";
            reportContent += $"Generated: {DateTime.Now}\n";
            reportContent += "No errors found.";

            return Encoding.UTF8.GetBytes(reportContent);
        }

        // ====================================================================
        // Get Execution Logs
        // ====================================================================
        public async Task<byte[]> GetExecutionLogsAsync(string executionId)
        {
            // TODO: Fetch logs from OSS Netact

            if (_executionCache.TryGetValue(executionId, out var status))
            {
                var logContent = new StringBuilder();
                logContent.AppendLine($"Execution Log: {executionId}");
                logContent.AppendLine($"Status: {status.Status}");
                logContent.AppendLine($"Total Sites: {status.Progress.TotalSites}");
                logContent.AppendLine($"Completed Sites: {status.Progress.CompletedSites}");
                logContent.AppendLine($"Failed Sites: {status.Progress.FailedSites}");
                logContent.AppendLine("");

                // ✅ Dùng StartedAt và CompletedAt THẬT
                if (status.StartedAt.HasValue)
                {
                    var startedLocal = TimeZoneInfo.ConvertTimeFromUtc(status.StartedAt.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    logContent.AppendLine($"Started: {startedLocal:MM/dd/yyyy h:mm:ss tt}");
                }

                if (status.CompletedAt.HasValue)
                {
                    var completedLocal = TimeZoneInfo.ConvertTimeFromUtc(status.CompletedAt.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    logContent.AppendLine($"Completed: {completedLocal:MM/dd/yyyy h:mm:ss tt}");
                }
            
                else
                {
                    logContent.AppendLine("Status: In progress...");
                }

                logContent.AppendLine("");
                logContent.AppendLine($"Message: {status.Message}");

                return Encoding.UTF8.GetBytes(logContent.ToString());
            }

            throw new KeyNotFoundException($"Execution not found: {executionId}");
        }
        // ====================================================================

        // ====================================================================
        // Execute Command on OSS Server via SSH
        // ====================================================================
        private async Task<string> ExecuteSSHCommandAsync(string command)
        {
            try
            {
                using (var sshClient = new SshClient(_sftpHost, _sftpPort, _sftpUsername, _sftpPassword))
                {
                    sshClient.Connect();
                    Console.WriteLine("✅ Connected to SSH server");

                    var cmd = sshClient.CreateCommand(command);
                    var result = cmd.Execute();

                    Console.WriteLine($"✅ Command executed: {command}");
                    Console.WriteLine($"📄 Output: {result}");

                    sshClient.Disconnect();

                    return result;
                }
                // ====================================================================

            }
            // ====================================================================

            catch (Exception ex)
            {
                Console.WriteLine($"❌ SSH Command Error: {ex.Message}");
                throw new Exception($"Failed to execute SSH command: {ex.Message}");
            }
            // ====================================================================

        }
        // ====================================================================



        // ====================================================================
        // Execute Single XML File on OSS
        // ====================================================================
        private async Task<(bool success, string error)> ExecuteSingleFileAsync(string remoteFilePath, string fileName)
        {
            try
            {
                Console.WriteLine($"🔵 Executing file: {fileName}");

                using (var sshClient = new SshClient(_sftpHost, _sftpPort, _sftpUsername, _sftpPassword))
                {
                    sshClient.Connect();

                    if (!sshClient.IsConnected)
                    {
                        return (false, "Cannot connect to SSH");
                    }

                    // Create unique plan name
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var planName = $"ZT_AUTO_{timestamp}_{Path.GetFileNameWithoutExtension(fileName)}";

                    // Build command
                    var command = $@"racclimx.sh -op ""Unified PnP - One-button planning"" " +
                                 $@"-planModeOptionalFileChooser {remoteFilePath} " +
                                 $@"-createPlan {planName} " +
                                 $@"-UIValues false " +
                                 $@"-fileFormat RAML2 " +
                                 $@"-v";

                    // Normalize command (remove \r\n)
                    command = command.Replace("\r", "").Replace("\n", " ");

                    Console.WriteLine($"📤 Command: {command}");

                    var cmd = sshClient.CreateCommand(command);
                    var result = cmd.Execute();

                    Console.WriteLine($"📥 Result: {result}");

                    sshClient.Disconnect();

                    // Check success
                    if (cmd.ExitStatus == 0 && result.Contains("Status received: Finished"))
                    {
                        Console.WriteLine($"✅ File executed successfully: {fileName}");
                        return (true, null);
                    }
                    else
                    {
                        Console.WriteLine($"❌ File execution failed: {fileName}");
                        return (false, $"Exit code: {cmd.ExitStatus}, Output: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception executing {fileName}: {ex.Message}");
                return (false, ex.Message);
            }
        }



        // ====================================================================
        // Execute Files in Batches (5 parallel)
        // ====================================================================
        private async Task ExecuteFilesInBatchesAsync(string executionId, List<string> xmlFiles)
        {
            var totalFiles = xmlFiles.Count;
            var completedFiles = 0;
            var failedFiles = 0;
            var batchSize = 5;

            Console.WriteLine($"🔵 Starting batch execution: {totalFiles} files, batch size: {batchSize}");

            // Process in batches of 5
            for (int i = 0; i < totalFiles; i += batchSize)
            {
                var batch = xmlFiles.Skip(i).Take(batchSize).ToList();

                Console.WriteLine($"🔵 Processing batch {i / batchSize + 1}: {batch.Count} files");

                // Execute batch in parallel
                var tasks = batch.Select(async filePath =>
                {
                    var fileName = Path.GetFileName(filePath);

                    // Update current site
                    _executionCache[executionId].Progress.CurrentSite = fileName;


                    var siteId = Path.GetFileNameWithoutExtension(fileName); // ← THÊM dòng này



                    // Execute file
                    var (success, error) = await ExecuteSingleFileAsync(filePath, fileName);

                    // Update counters
                    lock (_executionCache)
                    {

                        // ✅ THÊM: Lưu kết quả site
                        _executionCache[executionId].SiteResults.Add(new SiteExecutionResult
                        {
                            SiteId = siteId,
                            SiteName = fileName,
                            Status = success ? "success" : "failed",
                            Error = success ? null : error,
                            CompletedAt = DateTime.UtcNow
                        });

                        if (success)
                        {
                            completedFiles++;
                            _executionCache[executionId].Progress.CompletedSites = completedFiles;
                        }
                        else
                        {
                            failedFiles++;
                            _executionCache[executionId].Progress.FailedSites = failedFiles;
                        }

                        // Update percentage
                        var processed = completedFiles + failedFiles;
                        _executionCache[executionId].Progress.Percentage = (processed * 100) / totalFiles;
                        _executionCache[executionId].Message = $"Processing: {processed}/{totalFiles} sites";

                        Console.WriteLine($"📊 Progress: {processed}/{totalFiles} ({_executionCache[executionId].Progress.Percentage}%)");
                    }
                });

                // Wait for batch to complete
                await Task.WhenAll(tasks);
            }

            // Mark as completed
            _executionCache[executionId].Status = "completed";
            _executionCache[executionId].CompletedAt = DateTime.UtcNow; // ← THÊM field này
            _executionCache[executionId].Message = $"Completed: {completedFiles} success, {failedFiles} failed";

            Console.WriteLine($"✅ Execution completed at: {DateTime.UtcNow}");
        }



    }
    // ====================================================================
}
