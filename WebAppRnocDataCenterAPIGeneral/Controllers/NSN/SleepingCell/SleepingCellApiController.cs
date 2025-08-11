using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/sleeping-cell")]
    [Produces("application/json")]
    public class SleepingCellApiController : ControllerBase
    {
        private readonly InterfaceSleepingCellService _sleepingCellService;

        public SleepingCellApiController(InterfaceSleepingCellService sleepingCellService)
        {
            _sleepingCellService = sleepingCellService;
        }

        /// <summary>
        /// Get all sleeping cells for N8N workflow
        /// </summary>
        [HttpGet("list")]
        public async Task<ApiResponseDto<IEnumerable<SleepingCellDto>>> GetSleepingCells()
        {
            try
            {
                var cells = await _sleepingCellService.GetSleepingCellsAsync();

                return new ApiResponseDto<IEnumerable<SleepingCellDto>>
                {
                    Success = true,
                    Data = cells,
                    Message = $"Retrieved {cells.Count()} sleeping cells",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<IEnumerable<SleepingCellDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve sleeping cells",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get sleeping cell statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ApiResponseDto<SleepingCellStatsDto>> GetSleepingCellStats()
        {
            try
            {
                var stats = await _sleepingCellService.GetSleepingCellStatsAsync();

                return new ApiResponseDto<SleepingCellStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Retrieved sleeping cell statistics",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<SleepingCellStatsDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve statistics",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get cell status
        /// </summary>
        [HttpGet("cell/{cellName}/status")]
        public async Task<ApiResponseDto<CellStatusDto>> GetCellStatus(string cellName)
        {
            try
            {
                var status = await _sleepingCellService.GetCellStatusAsync(cellName);

                return new ApiResponseDto<CellStatusDto>
                {
                    Success = true,
                    Data = status,
                    Message = $"Retrieved status for cell {cellName}",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<CellStatusDto>
                {
                    Success = false,
                    Data = null,
                    Message = $"Failed to retrieve status for cell {cellName}",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }
    }
}