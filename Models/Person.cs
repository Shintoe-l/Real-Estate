using System;
using RealEstate.Models.Enums;

namespace RealEstate.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        public Role Role { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }
}
