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
    public class PropertyRepository(AppDbContext _context) : IPropertyRepository
    {
        public async Task<List<Property>> GetAllAsync()
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<Property?> GetByIdAsync(Guid id)
        {
            return await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Property>> GetByLandlordAsync(Guid landlordId)
        {
            return await _context.Properties
                .Where(p => p.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task<List<Property>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds)
        {
            return await _context.Properties
                .Where(p => propertyIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<Property> CreateAsync(Property property)
        {
            await _context.Properties.AddAsync(property);
            await _context.SaveChangesAsync();
            return property;
        }

        public async Task<Property?> UpdateAsync(Guid id, Property property)
        {
            var existing = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return null;

            existing.Address = property.Address;
            existing.City = property.City;
            existing.Province = property.Province;
            existing.Type = property.Type;
            existing.Bedrooms = property.Bedrooms;
            existing.Bathrooms = property.Bathrooms;
            existing.MonthlyRent = property.MonthlyRent;
            existing.Status = property.Status;
            existing.ListingType = property.ListingType;
            existing.ImageUrls = property.ImageUrls;
            existing.Description = property.Description;
            existing.Size = property.Size;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Property?> DeleteAsync(Guid id)
        {
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return null;

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return property;
        }

        public async Task<bool> PropertyExistsAsync(Guid id)
        {
            return await _context.Properties.AnyAsync(p => p.Id == id);
        }
    }
}
