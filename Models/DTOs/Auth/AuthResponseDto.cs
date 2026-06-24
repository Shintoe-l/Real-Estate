using RealEstate.Models.DTOs.Person;

namespace RealEstate.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public PersonDto User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
    }
}
