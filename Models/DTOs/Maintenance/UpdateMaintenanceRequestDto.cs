using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Maintenance
{
    public class UpdateMaintenanceRequestDto
    {
        public string? Description { get; set; }
        public MaintenanceStatus? Status { get; set; }
    }
}
