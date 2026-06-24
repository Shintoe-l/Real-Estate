using System;
using RealEstate.Models.Enums;

namespace RealEstate.Models
{
    public class MaintenanceRequest
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        
        public string Description { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        
        public MaintenanceStatus Status { get; set; }
    }
}
