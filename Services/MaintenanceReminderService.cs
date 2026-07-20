using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealEstate.Data;
using RealEstate.Helpers;
using RealEstate.Models.Enums;

namespace RealEstate.Services
{
    public class MaintenanceReminderService : BackgroundService
    {
        private readonly ILogger<MaintenanceReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Run interval set to 5 hours for production
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(5); 

        public MaintenanceReminderService(ILogger<MaintenanceReminderService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckUnresolvedRequestsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking unresolved maintenance requests.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Maintenance Reminder Service is stopping.");
        }

        private async Task CheckUnresolvedRequestsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get all maintenance requests that are not Resolved
            var unresolvedRequests = await context.MaintenanceRequests
                .Where(r => r.Status != MaintenanceStatus.Resolved)
                .ToListAsync();

            if (!unresolvedRequests.Any())
            {
                return;
            }

            foreach (var request in unresolvedRequests)
            {
                var property = await context.Properties.FindAsync(request.PropertyId);
                if (property == null) continue;

                var landlord = await context.Persons.FindAsync(property.LandlordId);
                if (landlord == null || string.IsNullOrEmpty(landlord.Email)) continue;

                var landlordName = $"{landlord.FirstName} {landlord.LastName}".Trim();
                if (string.IsNullOrEmpty(landlordName)) landlordName = "Landlord";

                EmailService.SendMaintenanceReminder(landlord.Email, landlordName, property.Address, request.Description);
            }
            
            _logger.LogInformation($"Processed reminders for {unresolvedRequests.Count} unresolved maintenance requests.");
        }
    }
}
