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
