using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models.DTOs.Lease;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaseController(ILeaseRepository _leaseRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leases = await _leaseRepo.GetAllAsync();
            var dtos = leases.Select(l => l.ToLeaseDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var lease = await _leaseRepo.GetByIdAsync(id);
            if (lease == null) return NotFound();
            return Ok(lease.ToLeaseDto());
        }

        /// <summary>
        /// Returns all active leases for a specific tenant directly from the database.
        /// </summary>
        [HttpGet("tenant/{tenantId:guid}")]
        public async Task<IActionResult> GetByTenant([FromRoute] Guid tenantId)
        {
            var leases = await _leaseRepo.GetByTenantAsync(tenantId);
            var dtos = leases.Select(l => l.ToLeaseDto());
            return Ok(dtos);
        }

        /// <summary>
        /// Returns total monthly rent obligation for a specific tenant, calculated from the database.
        /// </summary>
        [HttpGet("tenant/{tenantId:guid}/summary")]
        public async Task<IActionResult> GetTenantSummary([FromRoute] Guid tenantId)
        {
            var tenantLeases = await _leaseRepo.GetByTenantAsync(tenantId);

            var totalMonthlyObligation = tenantLeases.Sum(l => l.MonthlyRent);
            var activeLeaseCount = tenantLeases.Count;

            return Ok(new
            {
                tenantId,
                activeLeaseCount,
                totalMonthlyObligation
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var lease = dto.ToLeaseFromCreate();
            await _leaseRepo.CreateAsync(lease);
            return CreatedAtAction(nameof(GetById), new { id = lease.Id }, lease.ToLeaseDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLeaseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var lease = await _leaseRepo.GetByIdAsync(id);
            if (lease == null) return NotFound();
            lease.UpdateLease(dto);
            await _leaseRepo.UpdateAsync(id, lease);
            return Ok(lease.ToLeaseDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var lease = await _leaseRepo.DeleteAsync(id);
            if (lease == null) return NotFound();
            return NoContent();
        }
    }
}
