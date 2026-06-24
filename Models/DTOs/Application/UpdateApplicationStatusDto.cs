using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Application
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        public ApplicationStatus Status { get; set; }
    }
}
