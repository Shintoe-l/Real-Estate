using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Property
{
    public class UpdatePropertyDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public PropertyType? Type { get; set; }

        [Range(0, 20)]
        public int? Bedrooms { get; set; }

        [Range(0, 20)]
        public decimal? Bathrooms { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MonthlyRent { get; set; }

        public PropertyStatus? Status { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Description { get; set; }
        public int? Size { get; set; }
        public ListingType? ListingType { get; set; }
    }
}
