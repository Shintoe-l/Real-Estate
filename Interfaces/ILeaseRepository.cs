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
        Task<Lease> CreateAsync(Lease lease);
        Task<Lease?> UpdateAsync(Guid id, Lease lease);
        Task<Lease?> DeleteAsync(Guid id);
    }
}
