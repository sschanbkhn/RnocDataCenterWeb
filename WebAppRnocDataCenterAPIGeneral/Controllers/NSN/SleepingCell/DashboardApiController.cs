using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/dashboard")]
    [Produces("application/json")]
    public class DashboardApiController : ControllerBase
    {
        private readonly InterfaceDashboardService _dashboardService;

        public DashboardApiController(InterfaceDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // private readonly ConnectionsInformationSleepingCellDbContext _context;

        // public DashboardApiController(ConnectionsInformationSleepingCellDbContext context)
        // {
            // _context = context;
        // }

        /// <summary>
        /// Get dashboard summary for N8N monitoring
        /// </summary>
        [HttpGet("summary")]
        public async Task<ApiResponseDto<DashboardSummaryDto>> GetDashboardSummary()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();

                return new ApiResponseDto<DashboardSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = "Dashboard summary retrieved successfully",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<DashboardSummaryDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve dashboard summary",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get network health metrics
        /// </summary>
        [HttpGet("network-health")]
        public async Task<ApiResponseDto<NetworkHealthDto>> GetNetworkHealth()
        {
            try
            {
                var health = await _dashboardService.GetNetworkHealthAsync();

                return new ApiResponseDto<NetworkHealthDto>
                {
                    Success = true,
                    Data = health,
                    Message = "Network health retrieved successfully",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<NetworkHealthDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve network health",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get active alerts for N8N monitoring
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ApiResponseDto<IEnumerable<AlertDto>>> GetActiveAlerts()
        {
            try
            {
                var alerts = await _dashboardService.GetActiveAlertsAsync();

                return new ApiResponseDto<IEnumerable<AlertDto>>
                {
                    Success = true,
                    Data = alerts,
                    Message = $"Retrieved {alerts.Count()} active alerts",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<IEnumerable<AlertDto>>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve alerts",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get performance metrics for specified date range
        /// </summary>
        [HttpGet("performance")]
        public async Task<ApiResponseDto<PerformanceMetricsDto>> GetPerformanceMetrics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-7);
                var to = toDate ?? DateTime.UtcNow;

                var metrics = await _dashboardService.GetPerformanceMetricsAsync(from, to);

                return new ApiResponseDto<PerformanceMetricsDto>
                {
                    Success = true,
                    Data = metrics,
                    Message = $"Performance metrics from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PerformanceMetricsDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve performance metrics",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Get real-time system status
        /// </summary>
        [HttpGet("system-status")]
        public async Task<ApiResponseDto<Dictionary<string, object>>> GetSystemStatus()
        {
            try
            {
                var status = await _dashboardService.GetSystemStatusAsync();

                return new ApiResponseDto<Dictionary<string, object>>
                {
                    Success = true,
                    Data = status,
                    Message = "System status retrieved successfully",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Dictionary<string, object>>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve system status",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Check if system is healthy
        /// </summary>
        [HttpGet("health")]
        public async Task<ApiResponseDto<bool>> IsSystemHealthy()
        {
            try
            {
                var isHealthy = await _dashboardService.IsSystemHealthyAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = isHealthy,
                    Message = isHealthy ? "System is healthy" : "System has issues",
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
                    Message = "Failed to check system health",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }
        // ket thuc ham public async Task<ApiResponseDto<bool>> IsSystemHealthy()


        [HttpGet("trend")]
        // [HttpGet("summary/{trend}")]
        public async Task<ActionResult<Zone1Response>> funDashboardControllerGetSummaryTrend([FromQuery] string? endDate = null)
        {
            try
            {

                // var result = await _dashboardService.funDashboardServiceGetTrendAsync();
                // var result = await _dashboardService.funDashboardServiceGetTrendAsync(endDate);
                var result = await _dashboardService.funDashboardServiceGetTrendAsync(endDate);

                if (result.Success)
                    return Ok(result);  // ✅ Controller mới có Ok()
                else
                    return StatusCode(500, result);


            }
            /*
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting summary: {ex.Message}");
            }
            */

            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }


        }
        // ket thuc ham public async Task<ActionResult<Zone1Response>> GetSummary(string date)




        [HttpGet("summary/{date}")]
        public async Task<ActionResult<Zone1Response>> GetSummary(string date)
        {
            try
            {
                // Validate date format
                if (!DateOnly.TryParse(date, out var selectedDate))
                {
                    return BadRequest("Invalid date format. Use YYYY-MM-DD");
                }

                var summary = await _dashboardService.GetSummaryAsync(selectedDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting summary: {ex.Message}");
            }
        }
        // ket thuc ham public async Task<ActionResult<Zone1Response>> GetSummary(string date)


        // [HttpGet("province-summary")]
        // public async Task<ActionResult> funDashboardControllerGetProvinceSummary()
        [HttpGet("province-summary/{date}")]  // Thêm {date} parameter
        public async Task<IActionResult> GetProvinceSummary(string date)
        {
            try
            {
                // var result = await _dashboardService.funDashboardServiceGetProvinceSummary();  // ✅ Gọi đúng tên Service method
                var result = await _dashboardService.funDashboardServiceGetProvinceSummary(date);
                // return Ok(result);
                return Ok(result);
            }

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving province summary",
                    error = ex.Message
                });
            }



        }
        // ket thuc ham  public async Task<ActionResult> funDashboardControllerGetProvinceSummary()



        /// <summary>
        /// Get Zone 4 detailed table data for specified date
        /// </summary>
        [HttpGet("zone4-summary/{date}")]
        public async Task<IActionResult> funDashboardControllerGetZone4Summary(string date)
        {
            try
            {
                // Validate date format
                if (!DateOnly.TryParse(date, out var selectedDate))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid date format. Use YYYY-MM-DD"
                    });
                }

                var result = await _dashboardService.funDashboardServiceGetZone4SummaryAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving Zone 4 summary",
                    error = ex.Message
                });
            }
        }
        // ket thuc public async Task<IActionResult> funDashboardControllerGetZone4Summary(string date)

        // Trong DashboardApiController.cs
        [HttpGet("sleeping-cells/{date}")]
        public async Task<IActionResult> funDashboardControllerGetSleepingCells(string date)
        {
            try
            {
                var result = await _dashboardService.funDashboardServiceGetSleepingCellsAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
            
        }

        [HttpGet("process-cells/{date}")]
        public async Task<IActionResult> funDashboardControllerGetProcessCells(string date)
        {

            try
            {
                var result = await _dashboardService.funDashboardServiceGetProcessCellsAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }


        }

        [HttpGet("execution-cells/{date}")]
        public async Task<IActionResult> funDashboardControllerGetExecutionCells(string date)
        {

            try
            {
                var result = await _dashboardService.funDashboardServiceGetExecutionCellsAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }

        }

        [HttpGet("recheck-cells/{date}")]
        public async Task<IActionResult> funDashboardControllerGetRecheckCells(string date)
        {

            try
            {
                var result = await _dashboardService.funDashboardServiceGetRecheckCellsAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }




        }










    }
}