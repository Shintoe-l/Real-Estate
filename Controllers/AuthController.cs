using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.DTOs.Auth;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IPersonRepository _personRepo) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check if user already exists
            var existingUser = await _personRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "A user with this email already exists." });
            }

            // Create new person
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                PasswordHash = PasswordHasher.HashPassword(dto.Password)
            };

            await _personRepo.CreateAsync(person);

            var response = new AuthResponseDto
            {
                User = person.ToPersonDto(),
                Token = GenerateToken(person)
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var person = await _personRepo.GetByEmailAsync(dto.Email);
            if (person == null || !PasswordHasher.VerifyPassword(dto.Password, person.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var response = new AuthResponseDto
            {
                User = person.ToPersonDto(),
                Token = GenerateToken(person)
            };

            return Ok(response);
        }

        private static string GenerateToken(Person person)
        {
            var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
            var payload = $"{{\"sub\":\"{person.Id}\",\"email\":\"{person.Email}\",\"role\":\"{person.Role}\",\"exp\":{DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds()}}}";

            var headerBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(header))
                .Replace('+', '-').Replace('/', '_').Replace("=", "");
            var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload))
                .Replace('+', '-').Replace('/', '_').Replace("=", "");

            return $"{headerBase64}.{payloadBase64}.mock_signature";
        }
    }
}
