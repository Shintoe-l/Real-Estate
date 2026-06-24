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
        Task<Property> CreateAsync(Property property);
        Task<Property?> UpdateAsync(Guid id, Property property);
        Task<Property?> DeleteAsync(Guid id);
        Task<bool> PropertyExistsAsync(Guid id);
    }
}
