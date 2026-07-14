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

        /// <summary>Returns all applications submitted by a specific tenant.</summary>
        [HttpGet("tenant/{tenantId:guid}")]
        public async Task<IActionResult> GetByTenant([FromRoute] Guid tenantId)
        {
            var apps = await _appRepo.GetByTenantAsync(tenantId);
            var dtos = apps.Select(a => a.ToApplicationDto());
            return Ok(dtos);
        }

        /// <summary>Returns all applications for a landlord's properties (comma-separated property IDs).</summary>
        [HttpGet("by-properties")]
        public async Task<IActionResult> GetByPropertyIds([FromQuery] string propertyIds)
        {
            var ids = propertyIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : (Guid?)null)
                                 .Where(g => g.HasValue)
                                 .Select(g => g!.Value)
                                 .ToList();
            var apps = await _appRepo.GetByPropertyIdsAsync(ids);
            var dtos = apps.Select(a => a.ToApplicationDto());
            return Ok(dtos);
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

                 // If approved or acquired, perform automated operations based on application type
                 if (dto.Status == ApplicationStatus.Approved)
                 {
                     // For Rental applications, no lease is created yet until payment is verified (Acquired status)
                     // For ViewingRequest, no action needed here.
                 }
                 else if (dto.Status == ApplicationStatus.Acquired)
                 {
                     var property = await _propertyRepo.GetByIdAsync(app.PropertyId);
                     if (property != null)
                     {
                         // Mark property status based on application type
                         if (app.Type == ApplicationType.Purchase)
                         {
                             // Purchase = property is now Sold
                             property.Status = PropertyStatus.Sold;
                             await _propertyRepo.UpdateAsync(property.Id, property);

                             // Create a purchase agreement (lease record) so that payment can be linked to it
                             var existingPurchaseLeases = await _leaseRepo.GetByTenantAsync(app.TenantId);
                             var hasPurchaseLease = existingPurchaseLeases.Any(l => l.PropertyId == app.PropertyId);
                             if (!hasPurchaseLease)
                             {
                                 var purchaseLease = new Lease
                                 {
                                     Id = Guid.NewGuid(),
                                     PropertyId = app.PropertyId,
                                     TenantId = app.TenantId,
                                     StartDate = DateTime.UtcNow,
                                     EndDate = DateTime.UtcNow, // One-time purchase, not an ongoing lease
                                     MonthlyRent = property.MonthlyRent,
                                     SecurityDeposit = 0,
                                     IsActive = false // False = purchase agreement, not a rental lease
                                 };
                                 await _leaseRepo.CreateAsync(purchaseLease);
                             }
                         }
                         else if (app.Type == ApplicationType.Rental)
                         {
                             // Rental = property is now Occupied (rented)
                             property.Status = PropertyStatus.Occupied;
                             await _propertyRepo.UpdateAsync(property.Id, property);

                             // Create lease agreement if not already created
                             var existingLeases = await _leaseRepo.GetByTenantAsync(app.TenantId);
                             var hasLease = existingLeases.Any(l => l.PropertyId == app.PropertyId);
                             if (!hasLease)
                             {
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
                         }
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
