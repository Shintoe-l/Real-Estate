using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Person
{
    public class CreatePersonDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public Role Role { get; set; }
    }
}
