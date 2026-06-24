using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Interfaces;
using RealEstate.Models;

namespace RealEstate.Repositories
{
    public class LeaseRepository(AppDbContext _context) : ILeaseRepository
    {
        public async Task<List<Lease>> GetAllAsync()
        {
            return await _context.Leases.ToListAsync();
        }

        public async Task<Lease?> GetByIdAsync(Guid id)
        {
            return await _context.Leases.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Lease> CreateAsync(Lease lease)
        {
            await _context.Leases.AddAsync(lease);
            await _context.SaveChangesAsync();
            return lease;
        }

        public async Task<Lease?> UpdateAsync(Guid id, Lease lease)
        {
            var existing = await _context.Leases.FirstOrDefaultAsync(l => l.Id == id);
            if (existing == null) return null;

            existing.StartDate = lease.StartDate;
            existing.EndDate = lease.EndDate;
            existing.MonthlyRent = lease.MonthlyRent;
            existing.SecurityDeposit = lease.SecurityDeposit;
            existing.IsActive = lease.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Lease?> DeleteAsync(Guid id)
        {
            var lease = await _context.Leases.FirstOrDefaultAsync(l => l.Id == id);
            if (lease == null) return null;

            _context.Leases.Remove(lease);
            await _context.SaveChangesAsync();
            return lease;
        }
    }
}
