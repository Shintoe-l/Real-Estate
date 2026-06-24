using RealEstate.Models.Enums;

namespace RealEstate.Models.DTOs.Payment
{
    public class UpdatePaymentDto
    {
        public PaymentStatus? Status { get; set; }
    }
}
