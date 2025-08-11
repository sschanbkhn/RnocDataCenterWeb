using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using Microsoft.AspNetCore.Mvc;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{


    [ApiController]
    [Route("api/monitoring")]
    [Produces("application/json")]
    public class MonitoringApiController : ControllerBase
    {
        private readonly InterfaceMonitorService _kpiMonitorService;

        public MonitoringApiController(InterfaceMonitorService kpiMonitorService)
        {
            _kpiMonitorService = kpiMonitorService;
        }

        /// <summary>
        /// Get KPI Monitor data with pagination and filters
        /// </summary>
        [HttpGet("kpi-monitor/{date}")]
        public async Task<IActionResult> GetKpiMonitorData(
            string date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? region = null,
            [FromQuery] string? vendor = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
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

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Create request object
                var request = new KpiMonitorRequest
                {
                    Date = date,
                    Page = page,
                    PageSize = pageSize,
                    Province = province,
                    District = district,
                    Region = region,
                    Vendor = vendor,
                    Search = search,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                // Call service
                var result = await _kpiMonitorService.funMonitorServiceGetKpiMonitorDataAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving KPI Monitor data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get filter options for KPI Monitor dropdowns
        /// </summary>
        [HttpGet("kpi-monitor/filters")]
        public async Task<IActionResult> GetKpiMonitorFilters()
        {
            try
            {
                var result = await _kpiMonitorService.funMonitorServiceGetFiltersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving filter options",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Export KPI Monitor data to Excel
        /// </summary>
        [HttpGet("kpi-monitor/export/{date}")]
        public async Task<IActionResult> ExportKpiMonitorData(
            string date,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? region = null,
            [FromQuery] string? vendor = null,
            [FromQuery] string? search = null)
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

                // Create filter request (without pagination for export)
                var filters = new KpiMonitorRequest
                {
                    Date = date,
                    Province = province,
                    District = district,
                    Region = region,
                    Vendor = vendor,
                    Search = search
                };

                // Call service
                var fileBytes = await _kpiMonitorService.funMonitorServiceExportToExcelAsync(date, filters);

                var fileName = $"KPI_Monitor_{date}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error exporting KPI Monitor data",
                    error = ex.Message
                });
            }
        }


        /// <summary>
        /// Get KPI Monitor data with date range, pagination and filters
        /// </summary>
        [HttpGet("kpi-monitor-range")]
        public async Task<IActionResult> GetKpiMonitorDataRange(
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? region = null,
            [FromQuery] string? vendor = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            try
            {
                // Validate start date format
                if (!DateOnly.TryParse(startDate, out var parsedStartDate))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid start date format. Use YYYY-MM-DD"
                    });
                }

                // Validate end date format
                if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid end date format. Use YYYY-MM-DD"
                    });
                }

                // Validate date range
                if (parsedEndDate < parsedStartDate)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "End date cannot be earlier than start date"
                    });
                }

                // Validate date range is not too large (optional - prevent performance issues)
                var daysDifference = parsedEndDate.DayNumber - parsedStartDate.DayNumber;
                if (daysDifference > 31) // Max 31 days
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Date range cannot exceed 31 days"
                    });
                }

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Create request object
                var request = new KpiMonitorDateRangeRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Page = page,
                    PageSize = pageSize,
                    Province = province,
                    District = district,
                    Region = region,
                    Vendor = vendor,
                    Search = search,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                // Call service
                var result = await _kpiMonitorService.funMonitorServiceGetKpiMonitorDataRangeAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving KPI Monitor data range",
                    error = ex.Message
                });
            }
        }


















    }






}
