using System;
using System.ComponentModel.DataAnnotations;
using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Payment
{
    public class CreatePaymentDto
    {
        [Required]
        public Guid LeaseId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public PaymentType Type { get; set; }
    }
}
