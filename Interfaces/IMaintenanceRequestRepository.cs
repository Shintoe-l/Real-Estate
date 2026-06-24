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
        Task<MaintenanceRequest> CreateAsync(MaintenanceRequest request);
        Task<MaintenanceRequest?> UpdateAsync(Guid id, MaintenanceRequest request);
        Task<MaintenanceRequest?> DeleteAsync(Guid id);
    }
}
