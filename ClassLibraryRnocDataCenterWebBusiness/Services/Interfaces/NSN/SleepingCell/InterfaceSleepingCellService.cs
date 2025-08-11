using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;


using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;




namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell
{
    public interface InterfaceSleepingCellService
    {
        // Core operations for N8N/API
        Task<IEnumerable<SleepingCellDto>> GetSleepingCellsAsync();
        Task<IEnumerable<SleepingCellDto>> GetSleepingCellsByProvinceAsync(string province);
        Task<IEnumerable<SleepingCellDto>> GetSleepingCellsByVendorAsync(string vendor);
        Task<SleepingCellStatsDto> GetSleepingCellStatsAsync();

        // Single cell operations
        Task<CellStatusDto> GetCellStatusAsync(string cellName);
        Task<bool> IsCellEligibleForResetAsync(string cellName);

        // Reset operations
        Task<ResetResultDto> ResetSingleCellAsync(string cellName);
        Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames);
        Task<ResetValidationDto> ValidateResetRequestAsync(string cellName);
    }
}