using System;
using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Property
{
    public class CreatePropertyDto
    {
        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Province { get; set; } = string.Empty;

        public PropertyType Type { get; set; }

        [Range(0, 20)]
        public int Bedrooms { get; set; }

        [Range(0, 20)]
        public decimal Bathrooms { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyRent { get; set; }

        [Required]
        public Guid LandlordId { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public int Size { get; set; }
        public ListingType ListingType { get; set; }
    }
}
