using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Interfaces;
using RealEstate.Models;
using RealEstate.Models.Enums;

namespace RealEstate.Repositories
{
    public class ApplicationRepository(AppDbContext _context) : IApplicationRepository
    {
        public async Task<List<Application>> GetAllAsync()
        {
            return await _context.Applications.ToListAsync();
        }

        public async Task<Application?> GetByIdAsync(Guid id)
        {
            return await _context.Applications.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Application> CreateAsync(Application application)
        {
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<Application?> UpdateStatusAsync(Guid id, ApplicationStatus status)
        {
            var existing = await _context.Applications.FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null) return null;

            existing.Status = status;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Application?> DeleteAsync(Guid id)
        {
            var existing = await _context.Applications.FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null) return null;

            _context.Applications.Remove(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
