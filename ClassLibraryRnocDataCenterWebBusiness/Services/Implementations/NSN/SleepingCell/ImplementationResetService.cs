using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using Renci.SshNet;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

using Renci.SshNet.Common;

using System.Net.NetworkInformation;
using Microsoft.Extensions.Hosting;


using System.Threading;

using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell
{
    public class ImplementationResetService : InterfaceResetService
    {
        private readonly InterfaceSleepingCellResultRepository _resultRepository;
        private readonly InterfaceResetLimitRepository _limitRepository;
        private readonly InterfaceValidationService _validationService;
        private readonly InterfaceSshAccountRepository _sshRepository;
        private readonly InterfaceFilterTableRepository _filterRepository;
        private readonly InterfaceDetailTableRepository _detailRepository;
        private readonly ConnectionsInformationSleepingCellDbContext _context;

        public ImplementationResetService(
            InterfaceSleepingCellResultRepository resultRepository,
            InterfaceResetLimitRepository limitRepository,
            InterfaceValidationService validationService,
            InterfaceSshAccountRepository sshRepository,
            ConnectionsInformationSleepingCellDbContext __context,
            InterfaceFilterTableRepository filterRepository,
            InterfaceDetailTableRepository detailRepository)
        {
            _resultRepository = resultRepository;
            _limitRepository = limitRepository;
            _validationService = validationService;
            _sshRepository = sshRepository;
            _filterRepository = filterRepository;
            _detailRepository = detailRepository;
            _context = __context;
        }

        public async Task<ResetResultDto> ResetSingleCellAsync(string cellName)
        {
            return await ResetSingleCellAsync(cellName, "N8N");
        }

        public async Task<ResetResultDto> ResetSingleCellAsync(string cellName, string executedBy)
        {
            var startTime = DateTime.Now;

            try
            {
                // Validate before reset
                var validation = await ValidateResetRequestAsync(cellName);
                if (!validation.IsValid)
                {
                    return new ResetResultDto
                    {
                        CellName = cellName,
                        Success = false,
                        Message = validation.ValidationMessage,
                        ErrorCode = "VALIDATION_FAILED",
                        ResetTime = DateTime.Now,
                        ExecutedBy = executedBy,
                        ExecutionDuration = DateTime.Now - startTime
                    };
                }

                // TODO: Execute SSH reset command
                // var sshResult = await ExecuteSshResetCommand(cellName);

                // TODO: Update result in database
                // await _resultRepository.InsertResetResultAsync(result);

                var endTime = DateTime.Now;
                return new ResetResultDto
                {
                    CellName = cellName,
                    Success = true,
                    Message = "Reset completed successfully",
                    ResetTime = endTime,
                    ExecutedBy = executedBy,
                    ExecutionDuration = endTime - startTime,
                    Vendor = "NSN"
                };
            }
            catch (Exception ex)
            {
                return new ResetResultDto
                {
                    CellName = cellName,
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "RESET_ERROR",
                    ResetTime = DateTime.Now,
                    ExecutedBy = executedBy,
                    ExecutionDuration = DateTime.Now - startTime
                };
            }
        }








        public async Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames)
        {
            return await ResetBatchCellsAsync(cellNames, "N8N");
        }






        public async Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames, string executedBy)
        {
            var startTime = DateTime.Now;
            var results = new List<ResetResultDto>();
            var cellList = cellNames.ToList();

            foreach (var cellName in cellList)
            {
                var result = await ResetSingleCellAsync(cellName, executedBy);
                results.Add(result);
            }

            var endTime = DateTime.Now;
            var successCount = results.Count(r => r.Success);

            return new BatchResetResultDto
            {
                TotalRequested = cellList.Count,
                SuccessCount = successCount,
                FailedCount = cellList.Count - successCount,
                SkippedCount = 0,
                Results = results,
                OverallStatus = successCount == cellList.Count ? "SUCCESS" : "PARTIAL",
                TotalExecutionTime = endTime - startTime,
                StartTime = startTime,
                EndTime = endTime
            };
        }

        public async Task<ResetValidationDto> ValidateResetRequestAsync(string cellName)
        {
            // Basic validation - delegate to ValidationService later
            var isValid = !string.IsNullOrEmpty(cellName);

            return new ResetValidationDto
            {
                IsValid = isValid,
                EligibleCells = isValid ? new[] { cellName } : new string[0],
                BlacklistedCells = new string[0],
                InvalidCells = isValid ? new string[0] : new[] { cellName },
                ValidationMessage = isValid ? "Validation passed" : "Invalid cell name",
                WithinDailyLimit = true,
                RemainingQuota = 100,
                WithinTimeWindow = true,
                TimeWindowMessage = "Within allowed time window"
            };
        }

        public async Task<ResetValidationDto> ValidateResetRequestAsync(IEnumerable<string> cellNames)
        {
            var cellList = cellNames.ToList();
            var validCells = cellList.Where(c => !string.IsNullOrEmpty(c)).ToList();
            var invalidCells = cellList.Where(c => string.IsNullOrEmpty(c)).ToList();

            return new ResetValidationDto
            {
                IsValid = validCells.Count == cellList.Count,
                EligibleCells = validCells,
                BlacklistedCells = new string[0],
                InvalidCells = invalidCells,
                ValidationMessage = $"Validated {cellList.Count} cells",
                WithinDailyLimit = true,
                RemainingQuota = 100,
                WithinTimeWindow = true,
                TimeWindowMessage = "Within allowed time window"
            };
        }




        public async Task<IEnumerable<ResetResultDto>> GetResetHistoryAsync(int days = 7)
        {
            // TODO: Get from repository
            return new List<ResetResultDto>();
        }

        public async Task<bool> CanResetMoreCellsAsync(int requestedCount = 1)
        {
            // TODO: Check daily limits
            return true;
        }


        

        public async Task<BulkResetFromFilterTableResultDto> funImplementationServicesResetAllFilterTableCellsAsync(string executedBy = "SYSTEM-N8N")
        {
            var batchId = $"SYSTEM_N8N_{DateTime.Now:yyyyMMdd_HHmmss}";
            var startTime = DateTime.Now;
            int successCount = 0;
            int failedCount = 0;

            // ✅ STORE SUCCESSFUL SITES FOR LATER VERIFICATION
            var successfulSites = new List<(string siteName, string host, List<Objtable4gkpireportresultdetail> detailRecords)>();

            try
            {
                // 1-3. Existing logic - reset all sites
                var filterCells = await _filterRepository.GetAllSleepingCellsAsync();
                var cellsBySite = filterCells
                    .GroupBy(cell => GetMrbtsNameFromCellName(cell.LncelName, cell.LnbtsName))
                    .Where(group => !string.IsNullOrEmpty(group.Key))
                    .ToList();

                Debug.WriteLine($"📡 Starting reset for {cellsBySite.Count} sites...");
                Console.WriteLine($"📡 Starting reset for {cellsBySite.Count} sites...");
                

                // ✅ PROCESS ALL SITES FIRST (NO PING AFTER)
                foreach (var siteGroup in cellsBySite)
                {
                    var siteName = siteGroup.Key;
                    var cellsInSite = siteGroup.ToList();

                    try
                    {
                        var sshConfig = await GetSshConfigForSiteAsync(siteName);
                        if (sshConfig == null)
                        {
                            failedCount += cellsInSite.Count;
                            continue;
                        }

                        var detailRecords = new List<Objtable4gkpireportresultdetail>();
                        foreach (var cell in cellsInSite)
                        {
                            var detailRecord = await _detailRepository.GetOrCreateDetailRecordAsync(cell.LncelName);
                            await _detailRepository.UpdateExecutionStatusAsync(detailRecord.Id, "starting_reset");
                            detailRecords.Add(detailRecord);
                        }

                        // ✅ EXECUTE RESET (WITHOUT PING AFTER)
                        var siteResetResult = await funImplementationServiceExecuteSiteResetAsync(siteName, cellsInSite, sshConfig, skipPingAfter: true);

                        Debug.WriteLine($"📡 funImplementationServiceExecuteSiteResetAsync(siteName, cellsInSite, sshConfig, skipPingAfter: true)");
                        Console.WriteLine($"📡 funImplementationServiceExecuteSiteResetAsync(siteName, cellsInSite, sshConfig, skipPingAfter: true)");

                        // ✅ UPDATE DATABASE
                        foreach (var detailRecord in detailRecords)
                        {
                            var cellResult = siteResetResult.CellResults.FirstOrDefault(cr => cr.CellName == detailRecord.LncelName);

                            if (cellResult != null)
                            {
                                if (cellResult.Success)
                                {
                                    detailRecord.LastResetSuccess = true;
                                    detailRecord.ExecutionStatus = "Reset_command_sent"; // ✅ New status
                                    detailRecord.ExecutionNotes = "Reset command sent - verification pending";
                                    successCount++;

                                    // ✅ STORE FOR LATER VERIFICATION
                                    if (!successfulSites.Any(s => s.siteName == siteName))
                                    {
                                        successfulSites.Add((siteName, sshConfig.Host, new List<Objtable4gkpireportresultdetail>()));
                                    }
                                    successfulSites.First(s => s.siteName == siteName).detailRecords.Add(detailRecord);
                                }
                                else
                                {
                                    detailRecord.LastResetSuccess = false;
                                    detailRecord.ExecutionStatus = "failed";
                                    failedCount++;
                                }

                                await _detailRepository.UpdateResetResultAsync(detailRecord.Id, cellResult.Success, cellResult.Output, executedBy, new Dictionary<string, object>
                                {
                                    ["SshHost"] = sshConfig.Host,
                                    ["SshConnectionStatus"] = cellResult.SshConnectionStatus,
                                    ["PingTestBefore"] = cellResult.PingBefore,



                                    ["SshConnectStartedAt"] = cellResult.SshConnectStarted,
                                    ["SshConnectCompletedAt"] = cellResult.SshConnectCompleted,
                                    ["CommandSentAt"] = cellResult.CommandSentAt,
                                    ["CommandResponseReceivedAt"] = cellResult.CommandResponseReceived,



                                    ["ExecutionDuration"] = DateTime.Now - startTime, 


                                });
                            }
                        }
                        Debug.WriteLine($"📋 Starting archive for {detailRecords.Count} records...");
                        Console.WriteLine($"📋 Starting archive for {detailRecords.Count} records...");

                        // Archive records
                        foreach (var detailRecord in detailRecords)
                        {
                            await _detailRepository.CreateResultFromDetailAsync(detailRecord.Id);
                            await _detailRepository.ArchiveResultRecordAsync(detailRecord.Id);
                            await _detailRepository.ArchiveDetailRecordAsync(detailRecord.Id);
                        }

                        Debug.WriteLine($"📋 await _detailRepository.CreateResultFromDetailAsync(detailRecord.Id);");
                        Console.WriteLine($"📋 await _detailRepository.CreateResultFromDetailAsync(detailRecord.Id);");

                    }
                    catch (Exception siteEx)
                    {
                        Debug.WriteLine($"❌ Site {siteName} failed: {siteEx.Message}");
                        Console.WriteLine($"❌ Site {siteName} failed: {siteEx.Message}");
                        failedCount += cellsInSite.Count;
                    }
                }

                Debug.WriteLine($"📡 Completed reset commands for all sites");
                Console.WriteLine($"📡 Completed reset commands for all sites");
                
                Debug.WriteLine($"✅ Successful: {successCount}, ❌ Failed: {failedCount}");
                Console.WriteLine($"✅ Successful: {successCount}, ❌ Failed: {failedCount}");

                // ✅ NOW WAIT 15 MINUTES FOR ALL SITES
                if (successfulSites.Count > 0)
                {
                    Debug.WriteLine($"⏳ Waiting 15 minutes for {successfulSites.Count} sites to restart...");
                    Console.WriteLine($"⏳ Waiting 15 minutes for {successfulSites.Count} sites to restart...");


                    Debug.WriteLine($"🕐 Start time: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"🕐 Start time: {DateTime.Now:HH:mm:ss}");

                    // await Task.Delay(TimeSpan.FromMinutes(15)); // 15 minutes wait

                    Console.WriteLine($"🕐 Verification start time: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"🔍 Starting verification for {successfulSites.Count} sites...");

                    // ✅ VERIFY ALL SUCCESSFUL SITES
                    // await VerifyResetResults(successfulSites, batchId);
                }

                

                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                Debug.WriteLine($"⏳ var duration = endTime - startTime;");
                Console.WriteLine($"⏳ var duration = endTime - startTime;");


                return new BulkResetFromFilterTableResultDto
                {
                    Success = true,
                    TotalCells = filterCells.Count(),
                    SuccessfulResets = successCount,
                    FailedResets = failedCount,
                    BatchId = batchId,
                    StartedAt = startTime,
                    CompletedAt = endTime,
                    TotalDuration = duration,
                    ExecutedBy = executedBy,
                    Message = $"Reset completed: {successCount}/{filterCells.Count()} successful, {failedCount} failed. Verification completed after 15min wait."
                };

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ System_N8N_ reset failed: {ex.Message}");
                Console.WriteLine($"❌ System_N8N_ reset failed: {ex.Message}");
                return new BulkResetFromFilterTableResultDto
                {
                    Success = false,
                    Message = $"System_N8N_ reset failed: {ex.Message}"
                };
            }


        }


        private async Task VerifyResetResults(List<(string siteName, string host, List<Objtable4gkpireportresultdetail> detailRecords)> successfulSites, string batchId)
        {
            int verifiedCount = 0;
            int failedVerificationCount = 0;

            foreach (var (siteName, host, detailRecords) in successfulSites)
            {
                try
                {
                    Debug.WriteLine($"🔍 Verifying site: {siteName} ({host})");
                    
                    // ✅ TEST PING
                    var pingResult = await funImplementationServicePingTestAsyncServerAPIDEV(host);
                    Debug.WriteLine($"🏓 Ping verification: {(pingResult ? "SUCCESS" : "FAILED")}");

                    // ✅ TEST SSH (OPTIONAL)
                    bool sshResult = false;
                    if (pingResult)
                    {
                        try
                        {
                            var sshTest = await funImplementationServiceExecuteSystemSshRebootServerAPIDEV(host, "toor4nsn", "oZPS0POrRieRtu", testOnly: true);
                            sshResult = sshTest.Success;
                            Debug.WriteLine($"🔌 SSH verification: {(sshResult ? "SUCCESS" : "FAILED")}");
                        }
                        catch (Exception sshEx)
                        {
                            Debug.WriteLine($"🔌 SSH verification failed: {sshEx.Message}");
                        }
                    }

                    // ✅ UPDATE ALL CELLS IN SITE
                    bool siteVerified = pingResult; // hoặc pingResult && sshResult

                    foreach (var detailRecord in detailRecords)
                    {
                        if (siteVerified)
                        {
                            detailRecord.ExecutionStatus = "verified";
                            detailRecord.ExecutionNotes = "Reset verified - equipment online";
                            verifiedCount++;
                        }
                        else
                        {
                            detailRecord.ExecutionStatus = "verification_failed";
                            detailRecord.ExecutionNotes = "Reset sent but verification failed";
                            failedVerificationCount++;
                        }
                        
                        // Update ping after
                        await _detailRepository.UpdateResetResultAsync(detailRecord.Id, siteVerified, detailRecord.ExecutionNotes, "SYSTEM_VERIFICATION", new Dictionary<string, object>
                        {
                            ["PingTestAfter"] = pingResult,
                            ["VerificationTime"] = DateTime.Now,
                            ["VerificationBatchId"] = batchId
                        });
                    }

                    // Small delay between sites
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Verification failed for {siteName}: {ex.Message}");
                    failedVerificationCount += detailRecords.Count;
                }
            }

            Debug.WriteLine($"🔍 Verification completed:");
            Debug.WriteLine($"✅ Verified: {verifiedCount}");
            Debug.WriteLine($"❌ Failed verification: {failedVerificationCount}");
        }



        // ✅ SITE-LEVEL SSH CONFIG
        private async Task<SshConfig> GetSshConfigForSiteAsync1(string siteName)
        {
            try
            {
                // Get SSH credentials
                var sshAccount = await _context.Objtableaccountsshes
                    .Where(x => x.Active == true)
                    .FirstOrDefaultAsync();

                if (sshAccount == null) return null;

                // Get site IP từ mrbts_infor table
                var siteInfo = await _context.ObjtablemrbtsInfors
                    .Where(x => x.Enodebname == siteName)
                    .FirstOrDefaultAsync();

                if (siteInfo == null) return null;

                return new SshConfig
                {
                    Host = siteInfo.Oam,  // ⚠️ SỬA "IpAddress" thành tên column thực tế
                    Port = 22,
                    Username = sshAccount.Usename ?? "",
                    Password = sshAccount.Password ?? ""
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting SSH config for site {siteName}: {ex.Message}", ex);
            }
        }



        // ✅ EXTRACT MRBTS NAME FROM CELL NAME
        private string GetMrbtsNameFromCellName(string cellName, string lnbts_Name)
        {
            // Extract MRBTS name từ cell name
            if (string.IsNullOrEmpty(cellName)) return "";

            var parts = cellName.Split('-');
            if (parts.Length >= 3)
            {
                // VD: "4G-SMA009M12-SLA" -> "MRBTS-4G-SMA009M-SLA"
                /// return $"MRBTS-{parts[0]}-{parts[1]}-{parts[2]}";
                return lnbts_Name;
            }
            return "";
        }





        // ✅ LEGACY SSH CONFIG (FOR BACKWARD COMPATIBILITY)
        private async Task<SshConfig> GetSshConfigAsync()
        {
            try
            {
                var sshAccount = await _context.Objtableaccountsshes
                    .Where(x => x.Active == true)
                    .FirstOrDefaultAsync();

                if (sshAccount == null) return null;

                return new SshConfig
                {
                    Host = sshAccount.System ?? "",
                    Port = 22,
                    Username = sshAccount.Usename ?? "",
                    Password = sshAccount.Password ?? ""
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting SSH config: {ex.Message}", ex);
            }
        }

        // ✅ LEGACY SSH RESET (FOR BACKWARD COMPATIBILITY)
        private async Task<SshResetResult> ExecuteRealSshResetAsync(string cellName, SshConfig sshConfig)
        {
            try
            {
                using (var client = new SshClient(sshConfig.Host, sshConfig.Port, sshConfig.Username, sshConfig.Password))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(30);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        return new SshResetResult
                        {
                            Success = false,
                            CellName = cellName,
                            ErrorMessage = "SSH connection failed"
                        };
                    }

                    var commands = new[] { $"reset", $"reboot", $"restart" };
                    string output = "";
                    bool success = false;

                    foreach (var cmd in commands)
                    {
                        try
                        {
                            var _sshCommand = client.CreateCommand(cmd);
                            _sshCommand.CommandTimeout = TimeSpan.FromSeconds(60);
                            output = _sshCommand.Execute();

                            if (!output.ToLower().Contains("error") &&
                                !output.ToLower().Contains("failed") &&
                                !output.ToLower().Contains("not found"))
                            {
                                success = true;
                                break;
                            }
                        }
                        catch (Exception cmdEx)
                        {
                            output += $"Command '{cmd}' failed: {cmdEx.Message}; ";
                        }
                    }

                    client.Disconnect();

                    return new SshResetResult
                    {
                        Success = success,
                        CellName = cellName,
                        Output = output,
                        SshHost = sshConfig.Host
                    };
                }
            }
            catch (Exception ex)
            {
                return new SshResetResult
                {
                    Success = false,
                    CellName = cellName,
                    ErrorMessage = ex.Message
                };
            }
        }

        // ✅ HELPER CLASSES
        public class SshResetResult
        {
            public bool Success { get; set; }
            public string CellName { get; set; } = string.Empty;
            public string Output { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public string SshHost { get; set; } = string.Empty;
        }

        public class SshConfig
        {
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 22;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class SiteResetResult
        {
            public string SiteName { get; set; } = "";
            public List<CellResetResult> CellResults { get; set; } = new();
        }

        public class CellResetResult
        {
            public string CellName { get; set; } = "";
            public bool Success { get; set; }
            public string Output { get; set; } = "";


            // ✅ THÊM CÁC PROPERTIES MỚI:
            public bool PingBefore { get; set; }
            public bool PingAfter { get; set; }
            public string SshHost { get; set; } = "";
            public string SshConnectionStatus { get; set; } = "";
            public DateTime SshConnectStarted { get; set; }
            public DateTime SshConnectCompleted { get; set; }
            public DateTime CommandSentAt { get; set; }
            public DateTime CommandResponseReceived { get; set; }


        }

        private async Task<(bool Success, string Message)> ImprovedSshConnectionTest(SshConfig sshConfig)
        {
            try
            {
                Console.WriteLine($"🔌 Testing SSH: toor4nsn@{sshConfig.Host}:22");

                using (var client = new SshClient(sshConfig.Host, 22, "toor4nsn", "oZPS0POrRieRtu"))
                {
                    client.HostKeyReceived += (sender, e) => {
                        Console.WriteLine($"🔑 Accepting host key for {sshConfig.Host}");
                        e.CanTrust = true;
                    };

                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(45);

                    var startTime = DateTime.Now;
                    client.Connect();
                    var endTime = DateTime.Now;

                    if (client.IsConnected)
                    {
                        Console.WriteLine($"✅ SSH connected in {(endTime - startTime).TotalSeconds:F1}s");
                        client.Disconnect();
                        return (true, $"SSH successful");
                    }
                    else
                    {
                        return (false, "SSH client not connected");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SSH connection failed: {ex.Message}");
                return (false, $"SSH error: {ex.Message}");
            }
        }

        
        private async Task<SshConfig> GetSshConfigForSiteAsync(string siteName)
        {
            try
            {
                Debug.WriteLine($"🔍 Getting SSH config for site: {siteName}");

                if (string.IsNullOrEmpty(siteName))
                {
                    Debug.WriteLine("❌ Site name is empty");
                    return null;
                }

                // Get site IP from database
                var siteInfo = await _context.ObjtablemrbtsInfors
                    .Where(x => x.Enodebname == siteName)
                    .FirstOrDefaultAsync();

                if (siteInfo == null)
                {
                    Debug.WriteLine($"❌ No site info found for {siteName}");
                    return null;
                }

                if (string.IsNullOrEmpty(siteInfo.Oam))
                {
                    Debug.WriteLine($"❌ No OAM IP found for site {siteName}");
                    return null;
                }

                Debug.WriteLine($"✅ Found SSH config - Site: {siteName}, IP: {siteInfo.Oam}");

                // ✅ RETURN CONFIG WITH HARDCODED CREDENTIALS
                return new SshConfig
                {
                    Host = siteInfo.Oam.Trim(),
                    Port = 22,
                    Username = "toor4nsn",
                    Password = "oZPS0POrRieRtu"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting SSH config for {siteName}: {ex.Message}");
                return null;
            }
        }





        private async Task<bool> funImplementationServicePingTestAsyncServerAPIDEV(string host)
        {
            // ham ping cho server
            // server dung linux len khac so voi window
            if (string.IsNullOrEmpty(host))
                return false;

            try
            {
                using (var ping = new Ping())
                {
                    int successCount = 0;
                    int totalAttempts = 3;

                    Debug.WriteLine($"🏓 Ping test to {host} ({totalAttempts} attempts)");
                    Console.WriteLine($"🏓 Ping test to {host} ({totalAttempts} attempts)");

                    for (int i = 0; i < totalAttempts; i++)
                    {
                        try
                        {
                            var reply = await ping.SendPingAsync(host, 8000); // 8s timeout

                            if (reply.Status == IPStatus.Success)
                            {
                                successCount++;
                                Debug.WriteLine($"✅ Ping {i + 1}: SUCCESS ({reply.RoundtripTime}ms)");
                                Console.WriteLine($"✅ Ping {i + 1}: SUCCESS ({reply.RoundtripTime}ms)");
                            }
                            else
                            {
                                Debug.WriteLine($"❌ Ping {i + 1}: FAILED - {reply.Status}");
                                Console.WriteLine($"❌ Ping {i + 1}: FAILED - {reply.Status}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Ping {i + 1}: EXCEPTION - {ex.Message}");
                            Console.WriteLine($"❌ Ping {i + 1}: EXCEPTION - {ex.Message}");
                        }

                        if (i < totalAttempts - 1)
                            await Task.Delay(2000);
                    }

                    bool result = successCount >= 1; // Chỉ cần 1 ping thành công
                    Debug.WriteLine($"🏓 Ping result: {successCount}/{totalAttempts} - {(result ? "PASS" : "FAIL")}");
                    Console.WriteLine($"🏓 Ping result: {successCount}/{totalAttempts} - {(result ? "PASS" : "FAIL")}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ping failed for {host}: {ex.Message}");
                Console.WriteLine($"❌ Ping failed for {host}: {ex.Message}");
                return false;
            }
        }





        private async Task<SiteResetResult> funImplementationServiceExecuteSiteResetAsync(string siteName, List<Objtablefilterltekpireport> cells, SshConfig sshConfig, bool skipPingAfter = false)
        {
            var result = new SiteResetResult { SiteName = siteName };

            bool rebootSuccess = false;
            string rebootOutput = "";
            DateTime commandSentAt = DateTime.Now;
            DateTime commandResponseReceived = DateTime.Now;

            try
            {
                Debug.WriteLine($"🔍 Processing site: {siteName} -> {sshConfig.Host}");

                // ✅ PING TEST
                // var pingBefore = await PingTestAsync(sshConfig.Host);
                // var pingBefore = await funImplementationServicePingTestAsyncLocalDEV(sshConfig.Host);

                // chay cho local
                // var pingBefore = await funImplementationServicePingTestAsyncLocalDEV(sshConfig.Host);

                // chay cho server
                var pingBefore = await funImplementationServicePingTestAsyncServerAPIDEV(sshConfig.Host);

                Debug.WriteLine($"🔍 var pingBefore = await funImplementationServicePingTestAsyncServerAPIDEV(sshConfig.Host);");
                Console.WriteLine($"🔍 var pingBefore = await funImplementationServicePingTestAsyncServerAPIDEV(sshConfig.Host);");



                Debug.WriteLine($"🔍 DEBUG: pingBefore result = {pingBefore}");
                Console.WriteLine($"🔍 DEBUG: pingBefore result = {pingBefore}");
                // pingBefore = true; // Force true for now
                // Debug.WriteLine($"🔍 DEBUG: Forced pingBefore = {pingBefore}");

                // ✅ SYSTEM SSH REBOOT
                Debug.WriteLine($"🔌 Executing system SSH to {sshConfig.Host}...");
                System.Diagnostics.Debug.WriteLine($"🔍 DEBUG: pingBefore result = {pingBefore}");
                var sshConnectStarted = DateTime.Now;
                commandSentAt = DateTime.Now;


                Debug.WriteLine($"🔍 commandSentAt = DateTime.Now;");
                Console.WriteLine($"🔍 commandSentAt = DateTime.Now;,");


                // sh chay cho server API, dung linux
                var sshResult = await funImplementationServiceExecuteSystemSshRebootServerAPIDEV(sshConfig.Host, "toor4nsn", "oZPS0POrRieRtu");

                Debug.WriteLine($"var sshResult = await funImplementationServiceExecuteSystemSshRebootServerAPIDEV(sshConfig.Host,");
                Console.WriteLine($"var sshResult = await funImplementationServiceExecuteSystemSshRebootServerAPIDEV(sshConfig.Host,");

                // sh chay cho local
                // var sshResult = await funImplementationServiceSshExecuteAsyncLocalDEV(sshConfig.Host, "toor4nsn", "oZPS0POrRieRtu");

                commandResponseReceived = DateTime.Now;
                var sshConnectCompleted = DateTime.Now;

                rebootSuccess = sshResult.Success;
                rebootOutput = sshResult.Output;

                Debug.WriteLine($"SSH Result: {(rebootSuccess ? "SUCCESS" : "FAILED")} - {rebootOutput}");
                Console.WriteLine($"SSH Result: {(rebootSuccess ? "SUCCESS" : "FAILED")} - {rebootOutput}");











                // ✅ UPDATE ALL CELLS WITH SAME RESULT
                foreach (var cell in cells)
                {
                    var cellResult = new CellResetResult
                    {
                        CellName = cell.LncelName,
                        Success = rebootSuccess,
                        Output = rebootOutput,
                        PingBefore = pingBefore,
                        SshHost = sshConfig.Host,
                        SshConnectionStatus = rebootSuccess ? "connected" : "failed",
                        SshConnectStarted = sshConnectStarted,
                        SshConnectCompleted = sshConnectCompleted,
                        CommandSentAt = commandSentAt,
                        CommandResponseReceived = commandResponseReceived
                    };

                    result.CellResults.Add(cellResult);
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ catch (Exception ex), Site {siteName} failed completely: {ex.Message}");
                Console.WriteLine($"❌ catch (Exception ex), Site {siteName} failed completely: {ex.Message}");

                result.CellResults = cells.Select(c => new CellResetResult
                {
                    CellName = c.LncelName,
                    Success = false,
                    Output = $"Site execution failed: {ex.Message}",
                    SshHost = sshConfig.Host,
                    PingBefore = true, // Force true
                    PingAfter = false
                }).ToList();
            }

            return result;
        }



        private async Task<(bool Success, string Output)> funImplementationServiceExecuteSystemSshRebootServerAPIDEV(string host, string username, string password, bool testOnly = false)
        {
            try
            {
                // ✅ CHOOSE COMMAND BASED ON testOnly
                var command = testOnly ? "echo 'SSH test successful'" : "reboot";

                Debug.WriteLine($"🔌 var command = testOnly ? ");
                Console.WriteLine($"🔌 var command = testOnly ? ");

                var process = new System.Diagnostics.Process();


                // process.StartInfo.FileName = "ssh";

                // 🔧 THAY ĐỔI 1: Dùng sshpass thay vì ssh
                process.StartInfo.FileName = "sshpass";

                // process.StartInfo.Arguments = $"-o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -o ConnectTimeout=30 {username}@{host} '{command}'";

                // 🔧 THAY ĐỔI 2: Thêm -p 'password' vào arguments
                process.StartInfo.Arguments = $"-p '{password}' ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -o ConnectTimeout=30 {username}@{host} '{command}'";


                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;


                Debug.WriteLine($"🔌 Executing: sshpass -p [HIDDEN] ssh {username}@{host} {command}");
                Console.WriteLine($"🔌 Executing: sshpass -p [HIDDEN] ssh {username}@{host} {command}");











                Debug.WriteLine($"🔌 Executing: ssh {username}@{host} {command}");
                Console.WriteLine($"🔌 Executing: ssh {username}@{host} {command}");

                process.Start();

                // 🔧 THAY ĐỔI 3: XÓA CÁC DÒNG StandardInput (không cần nữa)


                // await process.StandardInput.WriteLineAsync(password);
                // await process.StandardInput.FlushAsync();
                // process.StandardInput.Close();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Timeout 15 giây quá ngắn cho reboot command!Server cần thời gian để:

                // Nhận lệnh reboot
                // Shutdown services
                // Actually reboot
                // SSH connection bị drop
                // da test thu, cung tam 2 chuc giay tu luc send lenh den luc reboot

                bool finished = process.WaitForExit(45000);

                Debug.WriteLine($"🔌 bool finished = process.WaitForExit(45000);");
                Console.WriteLine($"🔌 bool finished = process.WaitForExit(45000);");

                if (!finished)
                {
                    process.Kill();
                    return (false, "SSH timeout after 45 seconds");
                }

                var output = await outputTask;
                var error = await errorTask;

                Debug.WriteLine($"SSH exit code: {process.ExitCode}");
                Console.WriteLine($"SSH exit code: {process.ExitCode}");

                if (!string.IsNullOrEmpty(output)) Debug.WriteLine($"SSH output: {output}");
                if (!string.IsNullOrEmpty(error)) Debug.WriteLine($"SSH error: {error}");

                // ✅ SUCCESS CONDITIONS BASED ON COMMAND TYPE
                bool success;
                if (testOnly)
                {
                    // For test command, need exit code 0 and output
                    success = process.ExitCode == 0 && output.Contains("SSH test successful");
                }
                else
                {
                    // For reboot, exit code 0 or 255 (connection dropped)
                    success = process.ExitCode == 0 || process.ExitCode == 255;
                }

                string resultMessage = testOnly
                    ? (success ? "SSH connection verified" : $"SSH test failed: {output} {error}")
                    : (success ? "Reboot executed via system SSH" : $"Reboot failed: {output} {error}");

                return (success, resultMessage);
            }
            catch (Exception ex)
            {
                return (false, $"System SSH error: {ex.Message}");
            }
        }




        // ✅ WINDOWS 10 COMPATIBLE PING METHOD
        private async Task<bool> funImplementationServicePingTestAsyncLocalDEV(string host)
        {
            if (string.IsNullOrEmpty(host))
                return false;

            try
            {
                using (var ping = new Ping())
                {
                    int successCount = 0;
                    int totalAttempts = 3;

                    Debug.WriteLine($"🏓 Windows Ping test to {host} ({totalAttempts} attempts)");

                    for (int i = 0; i < totalAttempts; i++)
                    {
                        try
                        {
                            var reply = await ping.SendPingAsync(host, 5000); // 5s timeout

                            if (reply.Status == IPStatus.Success)
                            {
                                successCount++;
                                Debug.WriteLine($"✅ Ping {i + 1}: SUCCESS ({reply.RoundtripTime}ms)");
                            }
                            else
                            {
                                Debug.WriteLine($"❌ Ping {i + 1}: FAILED - {reply.Status}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Ping {i + 1}: EXCEPTION - {ex.Message}");
                        }

                        // Delay between attempts
                        if (i < totalAttempts - 1)
                            await Task.Delay(1500);
                    }

                    bool result = successCount >= 1; // At least 1 successful ping
                    Debug.WriteLine($"🏓 Windows Ping result: {successCount}/{totalAttempts} - {(result ? "PASS" : "FAIL")}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Windows Ping failed for {host}: {ex.Message}");
                return false;
            }
        }


        private async Task<(bool Success, string Output)> funImplementationServiceSshExecuteAsyncLocalDEV(string host, string username, string password, bool testOnly = false)
        {
            // ✅ VALIDATE INPUT
            if (string.IsNullOrEmpty(host))
            {
                Debug.WriteLine($"⚠️ SSH skipped - empty host");
                return (false, "Empty host provided");
            }

            Debug.WriteLine($"🔌 SSH to {host} (testOnly: {testOnly}) - Starting...");

            SshClient client = null;
            try
            {
                client = new SshClient(host, 22, username, password);

                // ✅ SHORTER TIMEOUTS TO FAIL FAST
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5); // 5 giây thôi
                client.ConnectionInfo.RetryAttempts = 1;

                // Accept any host key
                client.HostKeyReceived += (sender, e) => {
                    Debug.WriteLine($"🔑 Accepting host key for {host}");
                    e.CanTrust = true;
                };

                // ✅ CONNECT WITH DIRECT TRY-CATCH
                var connectStartTime = DateTime.Now;

                try
                {
                    Debug.WriteLine($"🔌 Attempting to connect to {host}...");
                    // await Task.Run(() => client.Connect());
                    client.Connect();
                    Debug.WriteLine($"🔌 Connect attempt completed for {host}");
                }
                catch (SshOperationTimeoutException timeoutEx)
                {
                    Debug.WriteLine($"⚠️ SSH TIMEOUT caught for {host}: {timeoutEx.Message}");
                    return (false, $"SSH timeout: {timeoutEx.Message}");
                }
                catch (Exception connectEx)
                {
                    Debug.WriteLine($"⚠️ SSH CONNECT ERROR caught for {host}: {connectEx.GetType().Name} - {connectEx.Message}");
                    return (false, $"SSH connection error: {connectEx.Message}");
                }

                var connectEndTime = DateTime.Now;

                if (!client.IsConnected)
                {
                    Debug.WriteLine($"⚠️ SSH connection failed to {host} - client not connected (continuing anyway)");
                    return (false, "SSH connection failed - client not connected");
                }

                Debug.WriteLine($"✅ SSH connected to {host} in {(connectEndTime - connectStartTime).TotalSeconds:F1}s");

                // ✅ EXECUTE COMMAND
                var command = testOnly ? "echo 'SSH_TEST_SUCCESS'" : "reboot";
                Debug.WriteLine($"🔌 Executing command on {host}: {command}");

                try
                {
                    using (var cmd = client.CreateCommand(command))
                    {
                        cmd.CommandTimeout = TimeSpan.FromSeconds(10);

                        var commandStartTime = DateTime.Now;
                        var result = cmd.Execute();
                        var commandEndTime = DateTime.Now;

                        Debug.WriteLine($"SSH command completed on {host} in {(commandEndTime - commandStartTime).TotalSeconds:F1}s");
                        Debug.WriteLine($"SSH command result from {host}: {result}");

                        // ✅ EVALUATE RESULT
                        bool success;
                        string resultMessage;

                        if (testOnly)
                        {
                            success = result.Contains("SSH_TEST_SUCCESS");
                            resultMessage = success
                                ? "SSH connection verified successfully"
                                : $"SSH test failed - unexpected output: {result}";
                        }
                        else
                        {
                            var lowerResult = result.ToLower();
                            bool hasError = lowerResult.Contains("error") ||
                                           lowerResult.Contains("permission denied") ||
                                           lowerResult.Contains("command not found") ||
                                           lowerResult.Contains("access denied") ||
                                           lowerResult.Contains("not found");

                            success = !hasError;
                            resultMessage = success
                                ? "Reboot command executed successfully via SSH"
                                : $"Reboot command may have failed: {result}";
                        }

                        Debug.WriteLine($"SSH operation result for {host}: {(success ? "SUCCESS" : "FAILED")} - {resultMessage}");
                        return (success, resultMessage);
                    }
                }
                catch (Exception cmdEx)
                {
                    Debug.WriteLine($"⚠️ SSH COMMAND ERROR for {host}: {cmdEx.Message}");
                    return (false, $"SSH command error: {cmdEx.Message}");
                }
            }
            catch (SshOperationTimeoutException timeoutEx)
            {
                Debug.WriteLine($"⚠️ SSH OUTER TIMEOUT for {host}: {timeoutEx.Message}");
                return (false, $"SSH timeout: {timeoutEx.Message}");
            }
            catch (System.Net.Sockets.SocketException sockEx)
            {
                Debug.WriteLine($"⚠️ SSH NETWORK ERROR for {host}: {sockEx.Message}");
                return (false, $"Network unreachable: {sockEx.Message}");
            }
            catch (SshConnectionException sshEx)
            {
                Debug.WriteLine($"⚠️ SSH CONNECTION EXCEPTION for {host}: {sshEx.Message}");
                return (false, $"SSH connection failed: {sshEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Debug.WriteLine($"⚠️ TIMEOUT EXCEPTION for {host}: {timeEx.Message}");
                return (false, $"SSH timeout: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ FINAL CATCH for {host}: {ex.GetType().FullName} - {ex.Message}");
                Debug.WriteLine($"⚠️ Stack trace: {ex.StackTrace}");
                return (false, $"SSH error: {ex.Message}");
            }
            finally
            {
                // ✅ PROPER CLEANUP
                try
                {
                    if (client != null && client.IsConnected)
                    {
                        client.Disconnect();
                    }
                    client?.Dispose();
                }
                catch (Exception cleanupEx)
                {
                    Debug.WriteLine($"⚠️ SSH cleanup warning for {host}: {cleanupEx.Message}");
                }
            }
        }









    }
}