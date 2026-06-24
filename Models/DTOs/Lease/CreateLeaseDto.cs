using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Models.DTOs.Lease
{
    public class CreateLeaseDto
    {
        [Required]
        public Guid PropertyId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyRent { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SecurityDeposit { get; set; }
    }
}
