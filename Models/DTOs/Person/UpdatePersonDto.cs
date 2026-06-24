using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Person
{
    public class UpdatePersonDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Role? Role { get; set; }
    }
}
