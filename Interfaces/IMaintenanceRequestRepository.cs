using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Models;

namespace RealEstate.Interfaces
{
    public interface IMaintenanceRequestRepository
    {
        Task<List<MaintenanceRequest>> GetAllAsync();
        Task<MaintenanceRequest?> GetByIdAsync(Guid id);
        Task<List<MaintenanceRequest>> GetByTenantAsync(Guid tenantId);
        Task<List<MaintenanceRequest>> GetByPropertyIdsAsync(IEnumerable<Guid> propertyIds);
        Task<MaintenanceRequest> CreateAsync(MaintenanceRequest request);
        Task<MaintenanceRequest?> UpdateAsync(Guid id, MaintenanceRequest request);
        Task<MaintenanceRequest?> DeleteAsync(Guid id);
    }
}
