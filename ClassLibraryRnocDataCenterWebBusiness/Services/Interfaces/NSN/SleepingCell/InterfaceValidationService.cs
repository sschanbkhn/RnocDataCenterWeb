using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell
{
    public interface InterfaceValidationService
    {
        // Business rule validation
        Task<ValidationResultDto> ValidateBusinessRulesAsync(string cellName);
        Task<ValidationResultDto> ValidateBusinessRulesAsync(IEnumerable<string> cellNames);

        // System validation
        Task<SystemValidationDto> ValidateSystemRequirementsAsync(string cellName);
        Task<BusinessRuleValidationDto> CheckBusinessRulesAsync(string cellName);

        // Pre-reset validation
        Task<PreResetCheckDto> PerformPreResetCheckAsync(string cellName);
        Task<BatchValidationDto> PerformBatchPreResetCheckAsync(IEnumerable<string> cellNames);

        // Individual validation checks
        Task<bool> IsBlacklistedAsync(string cellName);
        Task<bool> IsWithinDailyLimitAsync(int requestedCount = 1);
        Task<bool> IsWithinTimeWindowAsync();
        Task<bool> IsWithinVendorLimitAsync(string vendor, int requestedCount = 1);
        Task<bool> HasPassedCooldownPeriodAsync(string cellName);

        // Configuration validation
        Task<bool> IsCellConfigurationValidAsync(string cellName);
        Task<bool> IsSshConnectionAvailableAsync();
    }
}