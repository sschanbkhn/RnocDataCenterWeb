using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/nsn/sleeping-cell")]
    public class NsnSleepingCellBulkResetApiController : ControllerBase
    {
        private readonly InterfaceResetService _resetService;

        public NsnSleepingCellBulkResetApiController(InterfaceResetService resetService)
        {
            _resetService = resetService;
        }

        /// <summary>
        /// Bulk reset all sleeping cells from filter table
        /// N8N calls this single API to reset all cells
        /// </summary>
        [HttpPost("bulk-reset")]
        public async Task<IActionResult> BulkResetAllCells()
        {
            try
            {
                var result = await _resetService.ResetAllFilterTableCellsAsync("SYSTEM_N8N_RESET");

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = result,
                        message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        data = result,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"System-N8N reset failed: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Get status of filter table (optional - for checking before reset)
        /// </summary>
        [HttpGet("filter-table-status")]
        public async Task<IActionResult> GetFilterTableStatus()
        {
            try
            {
                // You can inject FilterTableRepository if needed for this endpoint
                return Ok(new
                {
                    success = true,
                    message = "Use bulk-reset endpoint to process all filter table cells"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}