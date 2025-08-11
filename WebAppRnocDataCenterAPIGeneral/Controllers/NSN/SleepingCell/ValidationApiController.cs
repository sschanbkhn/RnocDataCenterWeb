using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/validation")]
    [Produces("application/json")]
    public class ValidationApiController : ControllerBase
    {
        private readonly InterfaceValidationService _validationService;

        public ValidationApiController(InterfaceValidationService validationService)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Check business rules before reset - key N8N validation
        /// </summary>
        [HttpPost("check-business-rules")]
        public async Task<ApiResponseDto<ValidationResultDto>> CheckBusinessRules([FromBody] ValidationRequestDto request)
        {
            try
            {
                ValidationResultDto result;

                if (request.CellNames.Count() == 1)
                {
                    result = await _validationService.ValidateBusinessRulesAsync(request.CellNames.First());
                }
                else
                {
                    result = await _validationService.ValidateBusinessRulesAsync(request.CellNames);
                }

                return new ApiResponseDto<ValidationResultDto>
                {
                    Success = result.IsValid,
                    Data = result,
                    Message = result.OverallMessage,
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<ValidationResultDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Business rules validation failed",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Perform pre-reset check - comprehensive validation for N8N
        /// </summary>
        [HttpPost("pre-reset")]
        public async Task<ApiResponseDto<PreResetCheckDto>> PerformPreResetCheck([FromBody] string cellName)
        {
            try
            {
                var result = await _validationService.PerformPreResetCheckAsync(cellName);

                return new ApiResponseDto<PreResetCheckDto>
                {
                    Success = result.ReadyForReset,
                    Data = result,
                    Message = result.RecommendedAction,
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PreResetCheckDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Pre-reset validation failed",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Batch validation for multiple cells
        /// </summary>
        [HttpPost("batch-check")]
        public async Task<ApiResponseDto<BatchValidationDto>> PerformBatchValidation([FromBody] IEnumerable<string> cellNames)
        {
            try
            {
                var result = await _validationService.PerformBatchPreResetCheckAsync(cellNames);

                return new ApiResponseDto<BatchValidationDto>
                {
                    Success = result.BatchStatus == "READY",
                    Data = result,
                    Message = result.BatchSummary,
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<BatchValidationDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Batch validation failed",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Check if cell is blacklisted
        /// </summary>
        [HttpGet("blacklist/{cellName}")]
        public async Task<ApiResponseDto<bool>> IsBlacklisted(string cellName)
        {
            try
            {
                var isBlacklisted = await _validationService.IsBlacklistedAsync(cellName);

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = isBlacklisted,
                    Message = isBlacklisted ? "Cell is blacklisted" : "Cell is not blacklisted",
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
                    Message = "Failed to check blacklist status",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }

        /// <summary>
        /// Check daily limit status
        /// </summary>
        [HttpGet("daily-limit")]
        public async Task<ApiResponseDto<bool>> CheckDailyLimit([FromQuery] int requestedCount = 1)
        {
            try
            {
                var withinLimit = await _validationService.IsWithinDailyLimitAsync(requestedCount);

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = withinLimit,
                    Message = withinLimit ? "Within daily limit" : "Daily limit exceeded",
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
                    Message = "Failed to check daily limit",
                    Errors = new[] { ex.Message },
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                };
            }
        }
    }
}