using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Models.DTOs.Maintenance
{
    public class CreateMaintenanceRequestDto
    {
        [Required]
        public Guid PropertyId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}
