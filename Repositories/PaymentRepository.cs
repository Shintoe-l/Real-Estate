using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Interfaces;
using RealEstate.Models;

namespace RealEstate.Repositories
{
    public class PaymentRepository(AppDbContext _context) : IPaymentRepository
    {
        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Payment>> GetByLeaseIdsAsync(IEnumerable<Guid> leaseIds)
        {
            return await _context.Payments
                .Where(p => leaseIds.Contains(p.LeaseId))
                .ToListAsync();
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> UpdateAsync(Guid id, Payment payment)
        {
            var existing = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return null;

            existing.Amount = payment.Amount;
            existing.PaymentDate = payment.PaymentDate;
            existing.Type = payment.Type;
            existing.Status = payment.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Payment?> DeleteAsync(Guid id)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return null;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }
}
