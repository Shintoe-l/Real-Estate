using System;
using RealEstate.Models.Enums;

namespace RealEstate.Models
{
    public class Property
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        
        public PropertyType Type { get; set; }
        public int Bedrooms { get; set; }
        public decimal Bathrooms { get; set; }
        
        public decimal MonthlyRent { get; set; }
        public PropertyStatus Status { get; set; }
        public ListingType ListingType { get; set; }

        
        public Guid LandlordId { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public int Size { get; set; }


    }
}
