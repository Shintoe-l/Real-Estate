using System;
using RealEstate.Models.Enums;

namespace RealEstate.Models
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public ApplicationType Type { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
    }
}
