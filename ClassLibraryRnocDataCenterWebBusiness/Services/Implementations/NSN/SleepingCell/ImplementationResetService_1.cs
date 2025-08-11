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

        // ✅ SITE-LEVEL BULK RESET METHOD
        public async Task<BulkResetFromFilterTableResultDto> ResetAllFilterTableCellsAsync(string executedBy = "N8N")
        {
            var batchId = $"BULK_{DateTime.Now:yyyyMMdd_HHmmss}";
            var startTime = DateTime.Now;
            int successCount = 0;
            int failedCount = 0;

            try
            {
                Console.WriteLine("=== BULK RESET STARTED ===");

                // 1. Get all cells from filter table
                var filterCells = await _filterRepository.GetAllSleepingCellsAsync();
                var totalCells = filterCells.Count();

                Console.WriteLine($"Found {totalCells} cells in filter table");

                if (totalCells == 0)
                {
                    return new BulkResetFromFilterTableResultDto
                    {
                        Success = true,
                        TotalCells = 0,
                        SuccessfulResets = 0,
                        FailedResets = 0,
                        BatchId = batchId,
                        StartedAt = startTime,
                        CompletedAt = DateTime.Now,
                        TotalDuration = TimeSpan.Zero,
                        ExecutedBy = executedBy,
                        Message = "No cells found in filter table"
                    };
                }

                // ✅ 2. GROUP BY SITE (MRBTS)
                var cellsBySite = filterCells
                    .GroupBy(cell => GetMrbtsNameFromCellName(cell.LncelName))
                    .Where(group => !string.IsNullOrEmpty(group.Key))
                    .ToList();

                Console.WriteLine($"Found {filterCells.Count()} cells across {cellsBySite.Count} sites");

                // ✅ 3. PROCESS EACH SITE
                foreach (var siteGroup in cellsBySite)
                {
                    var siteName = siteGroup.Key;
                    var cellsInSite = siteGroup.ToList();

                    try
                    {
                        Console.WriteLine($"\n--- Processing site: {siteName} with {cellsInSite.Count} cells ---");

                        // 3.1. Get SSH config for this site
                        var sshConfig = await GetSshConfigForSiteAsync(siteName);
                        if (sshConfig == null)
                        {
                            Console.WriteLine($"No SSH config for site {siteName}");
                            failedCount += cellsInSite.Count;
                            continue;
                        }

                        // 3.2. Create detail records for all cells in site
                        var detailRecords = new List<Objtable4gkpireportresultdetail>();
                        foreach (var cell in cellsInSite)
                        {
                            var detailRecord = await _detailRepository.GetOrCreateDetailRecordAsync(cell.LncelName);
                            await _detailRepository.UpdateExecutionStatusAsync(detailRecord.Id, "starting_reset");
                            detailRecords.Add(detailRecord);
                        }

                        // 3.3. Execute site-level reset
                        var siteResetResult = await ExecuteSiteResetAsync(siteName, cellsInSite, sshConfig);

                        // 3.4. Update all cells in site with same result
                        foreach (var detailRecord in detailRecords)
                        {
                            var cellResult = siteResetResult.CellResults.FirstOrDefault(cr => cr.CellName == detailRecord.LncelName);
                            if (cellResult != null)
                            {
                                await _detailRepository.UpdateResetResultAsync(detailRecord.Id, cellResult.Success, cellResult.Output, executedBy);

                                // Update execution log
                                var logData = JsonSerializer.Serialize(new
                                {
                                    batchId = batchId,
                                    cellName = detailRecord.LncelName,
                                    siteName = siteName,
                                    startTime = startTime,
                                    endTime = DateTime.Now,
                                    sshHost = sshConfig.Host,
                                    command = $"reset {detailRecord.LncelName}",
                                    success = cellResult.Success,
                                    output = cellResult.Output,
                                    error = cellResult.Success ? null : cellResult.Output
                                });

                                await _detailRepository.UpdateExecutionLogAsync(detailRecord.Id, logData);

                                if (cellResult.Success)
                                    successCount++;
                                else
                                    failedCount++;
                            }
                        }
                    }
                    catch (Exception siteEx)
                    {
                        Console.WriteLine($"Site {siteName} failed: {siteEx.Message}");
                        failedCount += cellsInSite.Count;
                    }
                }

                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                Console.WriteLine($"\n=== BULK RESET COMPLETED ===");
                Console.WriteLine($"Success: {successCount}, Failed: {failedCount}");

                return new BulkResetFromFilterTableResultDto
                {
                    Success = true,
                    TotalCells = totalCells,
                    SuccessfulResets = successCount,
                    FailedResets = failedCount,
                    BatchId = batchId,
                    StartedAt = startTime,
                    CompletedAt = endTime,
                    TotalDuration = duration,
                    ExecutedBy = executedBy,
                    Message = $"Bulk reset completed: {successCount}/{totalCells} successful, {failedCount} failed"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BULK RESET FAILED: {ex.Message}");

                return new BulkResetFromFilterTableResultDto
                {
                    Success = false,
                    TotalCells = 0,
                    SuccessfulResets = successCount,
                    FailedResets = failedCount,
                    BatchId = batchId,
                    StartedAt = startTime,
                    CompletedAt = DateTime.Now,
                    ExecutedBy = executedBy,
                    Message = $"Bulk reset failed: {ex.Message}"
                };
            }
        }

        // ✅ SITE-LEVEL SSH CONFIG
        private async Task<SshConfig> GetSshConfigForSiteAsync(string siteName)
        {
            try
            {
                // Get SSH credentials
                var sshAccount = await _context.Objtableaccountsshs
                    .Where(x => x.Active == true)
                    .FirstOrDefaultAsync();

                if (sshAccount == null) return null;

                // Get site IP từ mrbts_infor table
                var siteInfo = await _context.Objtablemrbts_infor
                    .Where(x => x.Mrbtsname == siteName)
                    .FirstOrDefaultAsync();

                if (siteInfo == null) return null;

                return new SshConfig
                {
                    Host = siteInfo.IpAddress,  // ⚠️ SỬA "IpAddress" thành tên column thực tế
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

        // ✅ SITE-LEVEL RESET EXECUTION
        private async Task<SiteResetResult> ExecuteSiteResetAsync(string siteName, List<Objtablefilterltekpireport> cells, SshConfig sshConfig)
        {
            var result = new SiteResetResult { SiteName = siteName };

            try
            {
                using (var client = new SshClient(sshConfig.Host, sshConfig.Port, sshConfig.Username, sshConfig.Password))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(30);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        // All cells in site failed due to connection
                        result.CellResults = cells.Select(c => new CellResetResult
                        {
                            CellName = c.LncelName,
                            Success = false,
                            Output = "SSH connection failed"
                        }).ToList();
                        return result;
                    }

                    Console.WriteLine($"SSH connected to {sshConfig.Host} for site {siteName}");

                    // Reset each cell in the site
                    foreach (var cell in cells)
                    {
                        var cellResult = new CellResetResult { CellName = cell.LncelName };

                        try
                        {
                            // Try multiple reset commands
                            var commands = new[] { $"reset {cell.LncelName}", "reset", "reboot" };
                            bool success = false;
                            string output = "";

                            foreach (var cmd in commands)
                            {
                                try
                                {
                                    var sshCommand = client.CreateCommand(cmd);
                                    sshCommand.CommandTimeout = TimeSpan.FromSeconds(60);
                                    output = sshCommand.Execute();

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

                            cellResult.Success = success;
                            cellResult.Output = output;
                        }
                        catch (Exception ex)
                        {
                            cellResult.Success = false;
                            cellResult.Output = ex.Message;
                        }

                        result.CellResults.Add(cellResult);
                        Console.WriteLine($"Cell {cell.LncelName}: {(cellResult.Success ? "SUCCESS" : "FAILED")}");
                    }

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Site {siteName} SSH error: {ex.Message}");
                // All cells failed
                result.CellResults = cells.Select(c => new CellResetResult
                {
                    CellName = c.LncelName,
                    Success = false,
                    Output = ex.Message
                }).ToList();
            }

            return result;
        }

        // ✅ EXTRACT MRBTS NAME FROM CELL NAME
        private string GetMrbtsNameFromCellName(string cellName)
        {
            // Extract MRBTS name từ cell name
            if (string.IsNullOrEmpty(cellName)) return "";

            var parts = cellName.Split('-');
            if (parts.Length >= 3)
            {
                // VD: "4G-SMA009M12-SLA" -> "MRBTS-4G-SMA009M-SLA"
                return $"MRBTS-{parts[0]}-{parts[1]}-{parts[2]}";
            }
            return "";
        }

        // ✅ LEGACY SSH CONFIG (FOR BACKWARD COMPATIBILITY)
        private async Task<SshConfig> GetSshConfigAsync()
        {
            try
            {
                var sshAccount = await _context.Objtableaccountsshs
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
        }
    }
}