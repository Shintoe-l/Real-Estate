using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Models;
using RealEstate.Models.Enums;

namespace RealEstate.Interfaces
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetAllAsync();
        Task<Application?> GetByIdAsync(Guid id);
        Task<List<Application>> GetByTenantAsync(Guid tenantId);
        Task<List<Application>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds);
        Task<Application> CreateAsync(Application application);
        Task<Application?> UpdateStatusAsync(Guid id, ApplicationStatus status);
        Task<Application?> DeleteAsync(Guid id);
    }
}
