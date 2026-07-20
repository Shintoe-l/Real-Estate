using System.ComponentModel.DataAnnotations;

namespace RealEstate.Models.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
