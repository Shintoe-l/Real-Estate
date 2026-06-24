using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Interfaces;
using RealEstate.Mappers;
using RealEstate.Models.DTOs.Payment;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IPaymentRepository _paymentRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentRepo.GetAllAsync();
            var dtos = payments.Select(p => p.ToPaymentDto());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment.ToPaymentDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payment = dto.ToPaymentFromCreate();
            await _paymentRepo.CreateAsync(payment);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment.ToPaymentDto());
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePaymentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return NotFound();
            payment.UpdatePayment(dto);
            await _paymentRepo.UpdateAsync(id, payment);
            return Ok(payment.ToPaymentDto());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var payment = await _paymentRepo.DeleteAsync(id);
            if (payment == null) return NotFound();
            return NoContent();
        }
    }
}
