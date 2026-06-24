using System;

namespace RealEstate.Models
{
    public class Lease
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        
        public bool IsActive { get; set; }
    }
}
