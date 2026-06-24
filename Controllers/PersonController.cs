using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models.DTOs.Person;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(IPersonRepository _personRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var persons = await _personRepo.GetAllAsync();
            var dtos = persons.Select(p => p.ToPersonDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var person = await _personRepo.GetByIdAsync(id);
            if (person == null) return NotFound();
            return Ok(person.ToPersonDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePersonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var person = dto.ToPersonFromCreate();
            await _personRepo.CreateAsync(person);
            return CreatedAtAction(nameof(GetById), new { id = person.Id }, person.ToPersonDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePersonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var person = await _personRepo.GetByIdAsync(id);
            if (person == null) return NotFound();
            person.UpdatePerson(dto);
            await _personRepo.UpdateAsync(id, person);
            return Ok(person.ToPersonDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var person = await _personRepo.DeleteAsync(id);
            if (person == null) return NotFound();
            return NoContent();
        }
    }
}
