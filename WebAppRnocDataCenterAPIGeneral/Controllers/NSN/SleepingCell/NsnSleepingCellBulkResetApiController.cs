using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/nsn/sleeping-cell")]
    public class SleepingCellProcessResetApiController : ControllerBase
    {
        private readonly InterfaceResetService _resetService;

        public SleepingCellProcessResetApiController(InterfaceResetService resetService)
        {
            _resetService = resetService;
        }

        /// <summary>
        /// Bulk reset all sleeping cells from filter table
        /// N8N calls this single API to reset all cells
        /// </summary>
        [HttpPost("SleepingCell-reset")]
        public async Task<IActionResult> BulkResetAllCells()
        {
            try
            {
                // var result = await _resetService.funImplementationServicesResetAllFilterTableCellsAsync("SYSTEM_N8N_RESET");
                var result = await _resetService.funImplementationServicesResetAllFilterTableCellsAsync("SYSTEM_N8N_RESET");

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


        [HttpPost("SleepingCell-reset-mannual/{Ssh_Host_Ip_Mrbts}")]

            // public async Task<IActionResult> SingleResetAllCells([FromBody] ManualResetRequest request)
            public async Task<IActionResult> SingleResetAllCells(string Ssh_Host_Ip_Mrbts)
        {
            try
            {
                // var result = await _resetService.funImplementationServicesResetAllFilterTableCellsAsync("SYSTEM_N8N_RESET");
                // chi dung 2 API 


                if (string.IsNullOrEmpty(Ssh_Host_Ip_Mrbts))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Station Ssh IP address is required for reboot"
                    });
                }
                // Truyền trực tiếp IP vào service
                var result = await _resetService.funImplementationServicesSingleResetFilterCellsAsync(Ssh_Host_Ip_Mrbts);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Reset completed for IP: {Ssh_Host_Ip_Mrbts}"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Reset failed for IP: {Ssh_Host_Ip_Mrbts}"
                    });
                }


            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"User mannual reset failed: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        // Tạo file Models/ManualResetRequest.cs hoặc thêm vào file Models có sẵn
        /*
        public class ManualResetRequest
        {
            // public string? CellName { get; set; }
            // public string? MrbtsName { get; set; }
            // public int? MrbtsId { get; set; }  // ← Phải là int? (nullable)
            // public string TriggeredBy { get; set; } 
            // public string ResetReason { get; set; }
            // = "MANUAL_USER_RESET";
            public DateTime Timestamp { get; set; }

            public string Ssh_Host_IP { get; set; }  // Chỉ cần IP trạm
        }

        */


        /*

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
        */
    }
}