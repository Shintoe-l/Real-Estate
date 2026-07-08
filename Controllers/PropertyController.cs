using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models.DTOs.Property;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController(IPropertyRepository _propertyRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var properties = await _propertyRepo.GetAllAsync();
            var dtos = properties.Select(p => p.ToPropertyDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();

            return Ok(property.ToPropertyDto());
        }

        /// <summary>Returns all properties belonging to a specific landlord.</summary>
        [HttpGet("landlord/{landlordId:guid}")]
        public async Task<IActionResult> GetByLandlord([FromRoute] Guid landlordId)
        {
            var properties = await _propertyRepo.GetByLandlordAsync(landlordId);
            var dtos = properties.Select(p => p.ToPropertyDto());
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePropertyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var property = dto.ToPropertyFromCreate();
            await _propertyRepo.CreateAsync(property);

            return CreatedAtAction(nameof(GetById), new { id = property.Id }, property.ToPropertyDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePropertyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();

            property.UpdateProperty(dto);
            await _propertyRepo.UpdateAsync(id, property);

            return Ok(property.ToPropertyDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var property = await _propertyRepo.DeleteAsync(id);
            if (property == null) return NotFound();

            return NoContent();
        }
    }
}
