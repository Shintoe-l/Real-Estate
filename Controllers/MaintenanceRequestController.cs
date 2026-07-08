using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models.DTOs.Maintenance;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceRequestController(IMaintenanceRequestRepository _maintenanceRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _maintenanceRepo.GetAllAsync();
            var dtos = requests.Select(r => r.ToMaintenanceRequestDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var request = await _maintenanceRepo.GetByIdAsync(id);
            if (request == null) return NotFound();
            return Ok(request.ToMaintenanceRequestDto());
        }

        /// <summary>Returns all maintenance requests raised by a specific tenant.</summary>
        [HttpGet("tenant/{tenantId:guid}")]
        public async Task<IActionResult> GetByTenant([FromRoute] Guid tenantId)
        {
            var requests = await _maintenanceRepo.GetByTenantAsync(tenantId);
            var dtos = requests.Select(r => r.ToMaintenanceRequestDto());
            return Ok(dtos);
        }

        /// <summary>Returns all maintenance requests for a given set of property IDs.</summary>
        [HttpGet("by-properties")]
        public async Task<IActionResult> GetByPropertyIds([FromQuery] string propertyIds)
        {
            var ids = propertyIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : (Guid?)null)
                                 .Where(g => g.HasValue)
                                 .Select(g => g!.Value)
                                 .ToList();
            var requests = await _maintenanceRepo.GetByPropertyIdsAsync(ids);
            var dtos = requests.Select(r => r.ToMaintenanceRequestDto());
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var request = dto.ToMaintenanceRequestFromCreate();
            await _maintenanceRepo.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request.ToMaintenanceRequestDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMaintenanceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var request = await _maintenanceRepo.GetByIdAsync(id);
            if (request == null) return NotFound();
            request.UpdateMaintenanceRequest(dto);
            await _maintenanceRepo.UpdateAsync(id, request);
            return Ok(request.ToMaintenanceRequestDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var request = await _maintenanceRepo.DeleteAsync(id);
            if (request == null) return NotFound();
            return NoContent();
        }
    }
}
