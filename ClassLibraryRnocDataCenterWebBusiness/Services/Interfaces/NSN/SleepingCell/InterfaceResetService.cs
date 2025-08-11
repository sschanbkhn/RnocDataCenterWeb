using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;



namespace ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell
{
    public interface InterfaceResetService
    {
        // Core reset operations for N8N
        Task<ResetResultDto> ResetSingleCellAsync(string cellName);
        Task<ResetResultDto> ResetSingleCellAsync(string cellName, string executedBy);

        // Batch operations
        Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames);
        Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames, string executedBy);

        // Validation before reset
        Task<ResetValidationDto> ValidateResetRequestAsync(string cellName);
        Task<ResetValidationDto> ValidateResetRequestAsync(IEnumerable<string> cellNames);

        // Reset status and limits
        Task<IEnumerable<ResetResultDto>> GetResetHistoryAsync(int days = 7);
        Task<bool> CanResetMoreCellsAsync(int requestedCount = 1);

        // Thêm method này vào interface existing:
        Task<BulkResetFromFilterTableResultDto> ResetAllFilterTableCellsAsync(string executedBy = "N8N");


    }
}






