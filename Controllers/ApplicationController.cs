using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models;
using RealEstate.Models.DTOs.Application;
using RealEstate.Models.Enums;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController(
        IApplicationRepository _appRepo,
        IPropertyRepository _propertyRepo,
        ILeaseRepository _leaseRepo
    ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _appRepo.GetAllAsync();
            var dtos = apps.Select(a => a.ToApplicationDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var app = await _appRepo.GetByIdAsync(id);
            if (app == null) return NotFound();
            return Ok(app.ToApplicationDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verify property exists
            var property = await _propertyRepo.GetByIdAsync(dto.PropertyId);
            if (property == null) return BadRequest("Property does not exist.");

            // Create application
            var app = dto.ToApplicationFromCreate();
            await _appRepo.CreateAsync(app);

            return CreatedAtAction(nameof(GetById), new { id = app.Id }, app.ToApplicationDto());
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateApplicationStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var app = await _appRepo.GetByIdAsync(id);
            if (app == null) return NotFound("Application not found.");

            // Update status
            app.Status = dto.Status;
            await _appRepo.UpdateStatusAsync(id, dto.Status);

                // If approved, perform automated operations based on application type
                if (dto.Status == ApplicationStatus.Approved)
                {
                    var property = await _propertyRepo.GetByIdAsync(app.PropertyId);
                    if (property != null)
                    {
                        // For Rental applications, mark property occupied and create lease
                        if (app.Type == ApplicationType.Rental)
                        {
                            // 1. Mark property occupied
                            property.Status = PropertyStatus.Occupied;
                            await _propertyRepo.UpdateAsync(property.Id, property);

                            // 2. Automatically create a Lease Agreement
                            var lease = new Lease
                            {
                                Id = Guid.NewGuid(),
                                PropertyId = app.PropertyId,
                                TenantId = app.TenantId,
                                StartDate = DateTime.UtcNow,
                                EndDate = DateTime.UtcNow.AddYears(1),
                                MonthlyRent = property.MonthlyRent,
                                SecurityDeposit = property.MonthlyRent,
                                IsActive = true
                            };
                            await _leaseRepo.CreateAsync(lease);
                        }
                        // For ViewingRequest, no property status change or lease creation needed
                        // For Purchase, additional handling could be added later
                    }
                }

            return Ok(app.ToApplicationDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var app = await _appRepo.DeleteAsync(id);
            if (app == null) return NotFound();
            return NoContent();
        }
    }
}
