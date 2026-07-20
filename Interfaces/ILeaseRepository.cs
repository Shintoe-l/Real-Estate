using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Models;

namespace RealEstate.Interfaces
{
    public interface ILeaseRepository
    {
        Task<List<Lease>> GetAllAsync();
        Task<Lease?> GetByIdAsync(Guid id);
        Task<List<Lease>> GetByTenantAsync(Guid tenantId);
        Task<List<Lease>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds);
        Task<Lease> CreateAsync(Lease lease);
        Task<Lease?> UpdateAsync(Guid id, Lease lease);
        Task<Lease?> DeleteAsync(Guid id);
    }
}
