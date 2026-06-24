using System;

namespace RealEstate.Models.DTOs.Lease
{
    public class UpdateLeaseDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public bool? IsActive { get; set; }
    }
}
