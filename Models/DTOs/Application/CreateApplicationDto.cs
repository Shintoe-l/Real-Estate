using System;
using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Application
{
    public class CreateApplicationDto
    {
        [Required]
        public Guid PropertyId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public ApplicationType Type { get; set; }
    }
}
