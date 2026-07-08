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
    public class MaintenanceRequestRepository(AppDbContext _context) : IMaintenanceRequestRepository
    {
        public async Task<List<MaintenanceRequest>> GetAllAsync()
        {
            return await _context.MaintenanceRequests.ToListAsync();
        }

        public async Task<MaintenanceRequest?> GetByIdAsync(Guid id)
        {
            return await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MaintenanceRequest>> GetByTenantAsync(Guid tenantId)
        {
            return await _context.MaintenanceRequests
                .Where(m => m.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRequest>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds)
        {
            return await _context.MaintenanceRequests
                .Where(m => propertyIds.Contains(m.PropertyId))
                .ToListAsync();
        }

        public async Task<MaintenanceRequest> CreateAsync(MaintenanceRequest request)
        {
            await _context.MaintenanceRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<MaintenanceRequest?> UpdateAsync(Guid id, MaintenanceRequest request)
        {
            var existing = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (existing == null) return null;

            existing.Description = request.Description;
            existing.Status = request.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<MaintenanceRequest?> DeleteAsync(Guid id)
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (request == null) return null;

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return request;
        }
    }
}
