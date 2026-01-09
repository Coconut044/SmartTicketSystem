// Controllers/SlaController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SlaController : ControllerBase
    {
        private readonly ISlaService _slaService;

        public SlaController(ISlaService slaService)
        {
            _slaService = slaService;
        }

        /// <summary>
        /// Get all SLA configurations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<SlaConfigurationDto>>>> GetAll()
        {
            var slas = await _slaService.GetAllSlaConfigurationsAsync();

            return Ok(new ApiResponseDto<List<SlaConfigurationDto>>
            {
                Success = true,
                Message = "SLA configurations retrieved successfully",
                Data = slas
            });
        }

        /// <summary>
        /// Get SLA configuration by priority
        /// </summary>
        [HttpGet("priority/{priority}")]
        public async Task<ActionResult<ApiResponseDto<SlaConfigurationDto>>> GetByPriority(string priority)
        {
            var sla = await _slaService.GetSlaByPriorityAsync(priority);

            if (sla == null)
            {
                return NotFound(new ApiResponseDto<SlaConfigurationDto>
                {
                    Success = false,
                    Message = "SLA configuration not found for this priority"
                });
            }

            return Ok(new ApiResponseDto<SlaConfigurationDto>
            {
                Success = true,
                Message = "SLA configuration retrieved successfully",
                Data = sla
            });
        }

        /// <summary>
        /// Create SLA configuration - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<SlaConfigurationDto>>> Create([FromBody] CreateSlaConfigurationDto dto)
        {
            try
            {
                var sla = await _slaService.CreateSlaConfigurationAsync(dto);

                return CreatedAtAction(
                    nameof(GetByPriority),
                    new { priority = sla.Priority },
                    new ApiResponseDto<SlaConfigurationDto>
                    {
                        Success = true,
                        Message = "SLA configuration created successfully",
                        Data = sla
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<SlaConfigurationDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update SLA configuration - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<SlaConfigurationDto>>> Update(int id, [FromBody] CreateSlaConfigurationDto dto)
        {
            try
            {
                var sla = await _slaService.UpdateSlaConfigurationAsync(id, dto);

                return Ok(new ApiResponseDto<SlaConfigurationDto>
                {
                    Success = true,
                    Message = "SLA configuration updated successfully",
                    Data = sla
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<SlaConfigurationDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete (deactivate) SLA configuration - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
        {
            var result = await _slaService.DeleteSlaConfigurationAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "SLA configuration not found"
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "SLA configuration deleted successfully",
                Data = true
            });
        }
    }
}