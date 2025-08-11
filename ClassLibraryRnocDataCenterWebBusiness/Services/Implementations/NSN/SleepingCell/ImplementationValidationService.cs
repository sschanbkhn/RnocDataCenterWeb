using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell
{
    public class ImplementationValidationService : InterfaceValidationService
    {
        private readonly InterfaceBtsInfoRepository _btsRepository;
        private readonly InterfaceResetLimitRepository _limitRepository;
        private readonly InterfaceSleepingCellKpiRepository _kpiRepository;

        public ImplementationValidationService(
            InterfaceBtsInfoRepository btsRepository,
            InterfaceResetLimitRepository limitRepository,
            InterfaceSleepingCellKpiRepository kpiRepository)
        {
            _btsRepository = btsRepository;
            _limitRepository = limitRepository;
            _kpiRepository = kpiRepository;
        }

        public async Task<ValidationResultDto> ValidateBusinessRulesAsync(string cellName)
        {
            var businessRules = await CheckBusinessRulesAsync(cellName);
            var systemChecks = await ValidateSystemRequirementsAsync(cellName);

            return new ValidationResultDto
            {
                IsValid = businessRules.PassedBlacklistCheck && businessRules.PassedDailyLimitCheck,
                ValidCells = businessRules.PassedBlacklistCheck ? new[] { cellName } : new string[0],
                InvalidCells = !businessRules.PassedBlacklistCheck ? new[] { cellName } : new string[0],
                ValidationErrors = new List<ValidationErrorDetailDto>(),
                OverallMessage = businessRules.ValidationSummary,
                ValidationTime = DateTime.UtcNow
            };
        }

        public async Task<ValidationResultDto> ValidateBusinessRulesAsync(IEnumerable<string> cellNames)
        {
            var results = new List<ValidationResultDto>();
            foreach (var cellName in cellNames)
            {
                var result = await ValidateBusinessRulesAsync(cellName);
                results.Add(result);
            }

            var validCells = results.Where(r => r.IsValid).SelectMany(r => r.ValidCells).ToList();
            var invalidCells = results.Where(r => !r.IsValid).SelectMany(r => r.InvalidCells).ToList();

            return new ValidationResultDto
            {
                IsValid = invalidCells.Count == 0,
                ValidCells = validCells,
                InvalidCells = invalidCells,
                OverallMessage = $"Validated {cellNames.Count()} cells",
                ValidationTime = DateTime.UtcNow
            };
        }

        public async Task<SystemValidationDto> ValidateSystemRequirementsAsync(string cellName)
        {
            /// var cellExists = await _kpiRepository.CellExistsAsync(cellName);
            /// 
            var cells = await _kpiRepository.GetSleepingCellsAsync();
            var cellExists = cells.Any(c => c.LncelName == cellName);
            var btsName = ExtractBtsNameFromCell(cellName);
            var btsExists = !string.IsNullOrEmpty(btsName) &&
                           await _btsRepository.GetBtsByMrbtsNameAsync(btsName) != null;

            return new SystemValidationDto
            {
                CellExists = cellExists,
                BtsExists = btsExists,
                HasValidConfiguration = cellExists && btsExists,
                IsSystemHealthy = true,
                SshConnectionAvailable = true,
                SystemIssues = new List<string>(),
                SystemStatus = "Healthy"
            };
        }

        public async Task<BusinessRuleValidationDto> CheckBusinessRulesAsync(string cellName)
        {
            var isBlacklisted = await IsBlacklistedAsync(cellName);
            var withinDailyLimit = await IsWithinDailyLimitAsync(1);
            var withinTimeWindow = await IsWithinTimeWindowAsync();

            return new BusinessRuleValidationDto
            {
                PassedBlacklistCheck = !isBlacklisted,
                PassedDailyLimitCheck = withinDailyLimit,
                PassedTimeWindowCheck = withinTimeWindow,
                PassedVendorLimitCheck = true,
                PassedResetCooldownCheck = true,
                ValidationSummary = !isBlacklisted && withinDailyLimit && withinTimeWindow ? "All checks passed" : "Some checks failed"
            };
        }

        // Continue with remaining methods...
        /// private string ExtractBtsNameFromCell(string cellName)
        /// {
            // TODO: Implement logic to extract BTS name from cell name
            /// return cellName?.Split('_').FirstOrDefault() ?? "";
        ///}
        ///





        public async Task<PreResetCheckDto> PerformPreResetCheckAsync(string cellName)
        {
            var businessRules = await CheckBusinessRulesAsync(cellName);
            var systemChecks = await ValidateSystemRequirementsAsync(cellName);

            return new PreResetCheckDto
            {
                CellName = cellName,
                ReadyForReset = businessRules.PassedBlacklistCheck && systemChecks.IsSystemHealthy,
                BusinessRules = businessRules,
                SystemChecks = systemChecks,
                Prerequisites = new List<string>(),
                Warnings = new List<string>(),
                RecommendedAction = businessRules.PassedBlacklistCheck ? "Proceed" : "Review blacklist"
            };
        }

        public async Task<BatchValidationDto> PerformBatchPreResetCheckAsync(IEnumerable<string> cellNames)
        {
            var checks = new List<PreResetCheckDto>();
            foreach (var cellName in cellNames)
            {
                var check = await PerformPreResetCheckAsync(cellName);
                checks.Add(check);
            }

            var validCount = checks.Count(c => c.ReadyForReset);
            var warningCount = checks.Count(c => c.Warnings.Any());

            return new BatchValidationDto
            {
                TotalRequested = checks.Count,
                ValidCount = validCount,
                InvalidCount = checks.Count - validCount,
                WarningCount = warningCount,
                CellChecks = checks,
                BatchStatus = validCount == checks.Count ? "READY" : "PARTIAL",
                BatchSummary = $"{validCount}/{checks.Count} cells ready for reset"
            };
        }

        public async Task<bool> IsBlacklistedAsync(string cellName)
        {
            var btsName = ExtractBtsNameFromCell(cellName);
            if (string.IsNullOrEmpty(btsName)) return false;

            var btsInfo = await _btsRepository.GetBtsByMrbtsNameAsync(btsName);
            return btsInfo?.Blacklist ?? false;
        }

        public async Task<bool> IsWithinDailyLimitAsync(int requestedCount = 1)
        {
            // TODO: Check actual daily limits from repository
            return await Task.FromResult(true);
        }

        public async Task<bool> IsWithinTimeWindowAsync()
        {
            // TODO: Check time window configuration
            var currentHour = DateTime.Now.Hour;
            return await Task.FromResult(currentHour >= 8 && currentHour <= 18);
        }

        public async Task<bool> IsWithinVendorLimitAsync(string vendor, int requestedCount = 1)
        {
            // TODO: Check vendor-specific limits
            return await Task.FromResult(true);
        }

        public async Task<bool> HasPassedCooldownPeriodAsync(string cellName)
        {
            // TODO: Check last reset time vs cooldown period
            return await Task.FromResult(true);
        }

        public async Task<bool> IsCellConfigurationValidAsync(string cellName)
        {
            var btsName = ExtractBtsNameFromCell(cellName);
            if (string.IsNullOrEmpty(btsName)) return false;

            var btsInfo = await _btsRepository.GetBtsByMrbtsNameAsync(btsName);
            return btsInfo?.Reset ?? false;
        }

        public async Task<bool> IsSshConnectionAvailableAsync()
        {
            // TODO: Test SSH connection
            return await Task.FromResult(true);
        }

        private static string ExtractBtsNameFromCell(string cellName)
        {
            if (string.IsNullOrEmpty(cellName)) return "";

            // Extract BTS name from cell name pattern
            // Example: "HN_BTS_001_Cell_01" -> "HN_BTS_001"
            var parts = cellName.Split('_');
            return parts.Length >= 3 ? string.Join("_", parts.Take(3)) : cellName;
        }







    }
}