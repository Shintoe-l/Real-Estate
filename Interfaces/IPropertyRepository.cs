using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Models;

namespace RealEstate.Interfaces
{
    public interface IPropertyRepository
    {
        Task<List<Property>> GetAllAsync();
        Task<Property?> GetByIdAsync(Guid id);
        Task<List<Property>> GetByLandlordAsync(Guid landlordId);
        Task<List<Property>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds);
        Task<Property> CreateAsync(Property property);
        Task<Property?> UpdateAsync(Guid id, Property property);
        Task<Property?> DeleteAsync(Guid id);
        Task<bool> PropertyExistsAsync(Guid id);
    }
}
