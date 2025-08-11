


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

        private readonly ConnectionsInformationSleepingCellDbContext _context; // THÊM DÒNG NÀY


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
            _context = __context; // THÊM ASSIGNMENT


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

        // Continue với các methods khác...

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


        public async Task<BulkResetFromFilterTableResultDto> ResetAllFilterTableCellsAsync(string executedBy = "N8N")
        {
            var batchId = $"BULK_{DateTime.Now:yyyyMMdd_HHmmss}";
            var startTime = DateTime.Now;
            int successCount = 0;
            int failedCount = 0;

            try
            {
                // 1. Get all cells from filter table
                var filterCells = await _filterRepository.GetAllSleepingCellsAsync();
                var totalCells = filterCells.Count();

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
                        /// CompletedAt = DateTime.Now,
                        CompletedAt = DateTime.Now,
                        TotalDuration = TimeSpan.Zero,
                        ExecutedBy = executedBy,
                        Message = "No cells found in filter table"
                    };
                }

                // 2. Get SSH configuration
                var sshConfig = await GetSshConfigAsync();
                if (sshConfig == null)
                {
                    return new BulkResetFromFilterTableResultDto
                    {
                        Success = false,
                        TotalCells = totalCells,
                        SuccessfulResets = 0,
                        FailedResets = totalCells,
                        BatchId = batchId,
                        StartedAt = startTime,
                        CompletedAt = DateTime.Now,
                        ExecutedBy = executedBy,
                        Message = "No SSH configuration found"
                    };
                }

                // 3. Process each cell with real SSH
                foreach (var cell in filterCells)
                {
                    var cellStartTime = DateTime.Now;

                    try
                    {

                        Console.WriteLine($"Processing cell: {cell.LncelName}");

                        // 3.1. Create/get detail record
                        var detailRecord = await _detailRepository.GetOrCreateDetailRecordAsync(cell.LncelName);

                        // 3.2. Update status: starting
                        await _detailRepository.UpdateExecutionStatusAsync(detailRecord.Id, "starting_reset");

                        // 3.3. Update SSH connecting status
                        await _detailRepository.UpdateSshConnectionAsync(detailRecord.Id, "connecting", sshConfig.Host);

                        // 3.4. Execute real SSH reset
                        var sshResult = await ExecuteRealSshResetAsync(cell.LncelName, sshConfig);

                        // 3.5. Update SSH connection result
                        var connectionStatus = sshResult.Success ? "connected" : "failed";
                        await _detailRepository.UpdateSshConnectionAsync(detailRecord.Id, connectionStatus, sshConfig.Host);

                        // 3.6. Update reset result
                        await _detailRepository.UpdateResetResultAsync(detailRecord.Id, sshResult.Success, sshResult.Output, executedBy);

                        // 3.7. Update execution log
                        var logData = JsonSerializer.Serialize(new
                        {
                            batchId = batchId,
                            cellName = cell.LncelName,
                            startTime = cellStartTime,
                            endTime = DateTime.Now,
                            sshHost = sshConfig.Host,
                            command = $"reset {cell.LncelName}",
                            success = sshResult.Success,
                            output = sshResult.Output,
                            error = sshResult.ErrorMessage
                        });

                        await _detailRepository.UpdateExecutionLogAsync(detailRecord.Id, logData);

                        if (sshResult.Success)
                            successCount++;
                        else
                            failedCount++;

                    }
                    /// catch (Exception cellEx)

                    catch (Exception ex)
                    {
                        failedCount++;

                        var errorMessage = $"Error creating detail record for {cell.LncelName}: {ex.Message}";

                        if (ex.InnerException != null)
                        {
                            errorMessage += $" | Inner: {ex.InnerException.Message}";

                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" | Inner2: {ex.InnerException.InnerException.Message}";
                            }
                        }

                        // ✅ THROW ERROR VỀ API RESPONSE
                        throw new Exception(errorMessage, ex);


                        /// Console.WriteLine($"Cell {cell.LncelName} error: {cellEx.Message}");


                        // Log error for this cell
                        try
                        {
                            var detailRecord = await _detailRepository.GetOrCreateDetailRecordAsync(cell.LncelName);
                            /// await _detailRepository.UpdateResetResultAsync(detailRecord.Id, false, $"Exception: {cellEx.Message}", executedBy);
                            await _detailRepository.UpdateResetResultAsync(detailRecord.Id, false, $"Exception: {ex.Message}", executedBy);
                        }
                        catch
                        {
                            // If can't log, continue with next cell
                        }
                    }
                }

                var endTime = DateTime.Now;
                var duration = endTime - startTime;

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










        // Helper methods
        private async Task<SshResetResult> ExecuteRealSshResetAsync(string cellName, SshConfig sshConfig)
        {
            try
            {
                using (var client = new SshClient(sshConfig.Host, sshConfig.Port, sshConfig.Username, sshConfig.Password))
                {
                    // Set connection timeout
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

                    // Execute reset command - try multiple command variations
                    /// var commands = new[] { $"reset {cellName}", $"reboot {cellName}", $"restart {cellName}" };
                    /// var commands = new[] { $"reset {cellName}", $"reboot {cellName}", $"restart {cellName}" };
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

                            // Check if command was successful
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

        private async Task<SshConfig> GetSshConfigAsync()
        {
            /*
            try
            {
                var sshAccount = await _context.Objtableaccountsshs
                    .Where(x => x.Active == true)
                    .FirstOrDefaultAsync();

                if (sshAccount == null) return null;

                return new SshConfig
                {
                    Host = sshAccount.System ?? "",
                    /// Port = sshAccount.Port ?? 22,
                    /// Port = sshAccount.Port > 0 ? sshAccount.Port : 22,
                    /// 
                    /// Port = sshAccount.Port > 0 ? sshAccount.Port : 22,
                    /// 
                    Port = 22,
                    Username = sshAccount.Usename ?? "",
                    Password = sshAccount.Password ?? ""
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting SSH config: {ex.Message}", ex);
            }

            */





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
                    Host = siteInfo.IpAddress,  // ⚠️ SỬA "IpAddress" thành tên column thật
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

        // Helper classes
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

        // ✅ THÊM CÁC CLASSES MỚI
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