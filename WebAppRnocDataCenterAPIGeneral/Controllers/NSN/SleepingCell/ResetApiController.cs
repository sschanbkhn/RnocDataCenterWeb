using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/reset")]
    [Produces("application/json")]
    public class ResetApiController : ControllerBase
    {
        private readonly InterfaceResetService _resetService;

        public ResetApiController(InterfaceResetService resetService)
        {
            _resetService = resetService;
        }

        /// <summary>
        /// Reset single cell - main N8N operation
        /// </summary>
        [HttpPost("single")]
        public async Task<ApiResponseDto<ResetResultDto>> ResetSingleCell([FromBody] ResetRequestDto request)
        {
            try
            {
                var result = await _resetService.ResetSingleCellAsync(request.CellName, request.ExecutedBy);

                return new ApiResponseDto<ResetResultDto>
                {
                    Success = result.Success,
                    Data = result,
                    Message = result.Message,
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<ResetResultDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Reset operation failed",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Reset multiple cells in batch
        /// </summary>
        [HttpPost("batch")]
        public async Task<ApiResponseDto<BatchResetResultDto>> ResetBatchCells([FromBody] BatchResetRequestDto request)
        {
            try
            {
                var result = await _resetService.ResetBatchCellsAsync(request.CellNames, request.ExecutedBy);

                return new ApiResponseDto<BatchResetResultDto>
                {
                    Success = result.SuccessCount > 0,
                    Data = result,
                    Message = $"Batch reset completed: {result.SuccessCount}/{result.TotalRequested} successful",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<BatchResetResultDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Batch reset operation failed",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get reset history for monitoring
        /// </summary>
        [HttpGet("history")]
        public async Task<ApiResponseDto<IEnumerable<ResetResultDto>>> GetResetHistory([FromQuery] int days = 7)
        {
            try
            {
                var history = await _resetService.GetResetHistoryAsync(days);

                return new ApiResponseDto<IEnumerable<ResetResultDto>>
                {
                    Success = true,
                    Data = history,
                    Message = $"Retrieved reset history for last {days} days",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<IEnumerable<ResetResultDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve reset history",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Check if more resets can be performed (daily limit check)
        /// </summary>
        [HttpGet("can-reset")]
        public async Task<ApiResponseDto<bool>> CanResetMoreCells([FromQuery] int requestedCount = 1)
        {
            try
            {
                var canReset = await _resetService.CanResetMoreCellsAsync(requestedCount);

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = canReset,
                    Message = canReset ? $"Can reset {requestedCount} more cells" : "Daily limit reached",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Failed to check reset limits",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }
    }
}