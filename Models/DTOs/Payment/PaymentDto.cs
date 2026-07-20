using System;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid LeaseId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentType Type { get; set; }
        public PaymentMethod Method { get; set; }
        public string? Reference { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
