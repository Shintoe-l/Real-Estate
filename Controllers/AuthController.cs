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

            // Generate a 6-digit confirmation code
            var code = new Random().Next(100000, 999999).ToString();

            // Create new person
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                IsEmailConfirmed = false,
                EmailConfirmationCode = code
            };

            await _personRepo.CreateAsync(person);

            // "Send" email verification code
            EmailService.SendVerificationCode(person.Email, code);

            return Ok(new { message = "Registration successful. Please verify your email.", email = person.Email });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var person = await _personRepo.GetByEmailAsync(dto.Email);
            if (person == null) return NotFound(new { message = "User not found." });

            if (person.EmailConfirmationCode != dto.Code)
            {
                return BadRequest(new { message = "Invalid verification code." });
            }

            person.IsEmailConfirmed = true;
            person.EmailConfirmationCode = null;
            await _personRepo.UpdateAsync(person.Id, person);

            var response = new AuthResponseDto
            {
                User = person.ToPersonDto(),
                Token = GenerateToken(person)
            };

            return Ok(response);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var person = await _personRepo.GetByEmailAsync(dto.Email);
            if (person == null) return NotFound(new { message = "User not found." });

            if (person.IsEmailConfirmed)
            {
                return BadRequest(new { message = "Email is already verified." });
            }

            var code = new Random().Next(100000, 999999).ToString();
            person.EmailConfirmationCode = code;
            await _personRepo.UpdateAsync(person.Id, person);

            EmailService.SendVerificationCode(person.Email, code);

            return Ok(new { message = "Verification code resent." });
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

            if (!person.IsEmailConfirmed)
            {
                return BadRequest(new { message = "Please verify your email address before logging in.", email = person.Email, requiresVerification = true });
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
