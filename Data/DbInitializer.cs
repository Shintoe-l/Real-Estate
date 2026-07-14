using System;
using System.Linq;
using RealEstate.Models;
using RealEstate.Models.Enums;
using RealEstate.Helpers;

namespace RealEstate.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Properties.Any() || context.Persons.Any())
            {
                return; // DB has been seeded
            }

            // 1. Seed People (Landlord, PM, Tenants)
            var landlord = new Person
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@realestate.com",
                PhoneNumber = "+1234567890",
                Role = Role.Landlord,
                PasswordHash = PasswordHasher.HashPassword("Password123!")
            };

            var pm = new Person
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Marcus",
                LastName = "Aurelius",
                Email = "marcus.pm@tbeeestates.com",
                PhoneNumber = "+1555909808",
                Role = Role.PropertyManager,
                PasswordHash = PasswordHasher.HashPassword("Password123!")
            };

            var tenant1 = new Person
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@gmail.com",
                PhoneNumber = "+1987654321",
                Role = Role.Tenant,
                PasswordHash = PasswordHasher.HashPassword("Password123!")
            };

            var tenant2 = new Person
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                FirstName = "Alice",
                LastName = "Cooper",
                Email = "alice.cooper@gmail.com",
                PhoneNumber = "+1444888999",
                Role = Role.Tenant,
                PasswordHash = PasswordHasher.HashPassword("Password123!")
            };

            context.Persons.AddRange(landlord, pm, tenant1, tenant2);

            // 2. Seed Beautiful Real Estate Properties
            var prop1 = new Property
            {
                Id = Guid.NewGuid(),
                Address = "123 Ocean Drive",
                City = "Miami",
                Province = "FL",
                Type = PropertyType.Residential,
                Bedrooms = 2,
                Bathrooms = 2.0m,
                MonthlyRent = 2500.00m,
                Status = PropertyStatus.Occupied, // Active lease exists
                ListingType = ListingType.ForRent,
                LandlordId = landlord.Id,
                ImageUrls = new List<string>(),
                Description = "A magnificent modern beachfront apartment offering sweeping ocean views, an expansive private balcony, and direct access to Miami Beach.",
                Size = 1250
            };

            var prop2 = new Property
            {
                Id = Guid.NewGuid(),
                Address = "456 Skyline Boulevard",
                City = "Seattle",
                Province = "WA",
                Type = PropertyType.Residential,
                Bedrooms = 3,
                Bathrooms = 2.5m,
                MonthlyRent = 3800.00m,
                Status = PropertyStatus.Occupied, // Active lease exists
                ListingType = ListingType.ForRent,
                LandlordId = landlord.Id,
                ImageUrls = new List<string>(),
                Description = "An industrial-chic urban loft located in the heart of downtown. Features double-height ceilings, exposed brick walls, premium smart-home integration, and a stunning rooftop terrace.",
                Size = 1850
            };

            var prop3 = new Property
            {
                Id = Guid.NewGuid(),
                Address = "789 Tech Corridor Lane",
                City = "Bellevue",
                Province = "WA",
                Type = PropertyType.Commercial,
                Bedrooms = 5,
                Bathrooms = 4.0m,
                MonthlyRent = 8500.00m,
                Status = PropertyStatus.Available, // Open for leasing
                ListingType = ListingType.ForRent,
                LandlordId = pm.Id, // Owned by Property Manager Marcus
                ImageUrls = new List<string>(),
                Description = "State-of-the-art boutique commercial office space designed for high-performing teams. High-speed fiber connectivity, meeting boardrooms, and a private corporate lounge.",
                Size = 4200
            };

            var prop4 = new Property
            {
                Id = Guid.NewGuid(),
                Address = "12 Emerald Lake Road",
                City = "Aspen",
                Province = "CO",
                Type = PropertyType.Residential,
                Bedrooms = 4,
                Bathrooms = 4.5m,
                MonthlyRent = 6200.00m,
                Status = PropertyStatus.Available, // Open for leasing
                ListingType = ListingType.ForRent,
                LandlordId = landlord.Id,
                ImageUrls = new List<string>(),
                Description = "An ultra-luxury alpine chalet boasting rustic-modern architecture, floor-to-ceiling panoramic windows, an outdoor heated hot tub, and ski-in/ski-out convenience.",
                Size = 3500
            };

            context.Properties.AddRange(prop1, prop2, prop3, prop4);

            // 3. Seed Leases & Contracts
            var lease1 = new Lease
            {
                Id = Guid.NewGuid(),
                PropertyId = prop1.Id,
                TenantId = tenant1.Id,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(305),
                MonthlyRent = 2500.00m,
                SecurityDeposit = 2500.00m,
                IsActive = true
            };

            var lease2 = new Lease
            {
                Id = Guid.NewGuid(),
                PropertyId = prop2.Id,
                TenantId = tenant2.Id,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(335),
                MonthlyRent = 3800.00m,
                SecurityDeposit = 3800.00m,
                IsActive = true
            };

            context.Leases.AddRange(lease1, lease2);

            // 4. Seed Payments Ledger History
            var p1 = new Payment
            {
                Id = Guid.NewGuid(),
                LeaseId = lease1.Id,
                Amount = 2500.00m, // Deposit
                PaymentDate = DateTime.UtcNow.AddDays(-60),
                Type = PaymentType.SecurityDeposit,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed
            };

            var p2 = new Payment
            {
                Id = Guid.NewGuid(),
                LeaseId = lease1.Id,
                Amount = 2500.00m, // Month 1 rent
                PaymentDate = DateTime.UtcNow.AddDays(-60),
                Type = PaymentType.Rent,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed
            };

            var p3 = new Payment
            {
                Id = Guid.NewGuid(),
                LeaseId = lease1.Id,
                Amount = 2500.00m, // Month 2 rent
                PaymentDate = DateTime.UtcNow.AddDays(-30),
                Type = PaymentType.Rent,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed
            };

            var p4 = new Payment
            {
                Id = Guid.NewGuid(),
                LeaseId = lease2.Id,
                Amount = 3800.00m, // Month 1 rent
                PaymentDate = DateTime.UtcNow.AddDays(-30),
                Type = PaymentType.Rent,
                Method = PaymentMethod.CreditCard,
                Status = PaymentStatus.Completed
            };

            context.Payments.AddRange(p1, p2, p3, p4);

            // 5. Seed Maintenance Operations History
            var req1 = new MaintenanceRequest
            {
                Id = Guid.NewGuid(),
                PropertyId = prop1.Id,
                TenantId = tenant1.Id,
                Description = "The HVAC system requires a routine filter replacement and compressor diagnostics.",
                RequestDate = DateTime.UtcNow.AddDays(-45),
                Status = MaintenanceStatus.Resolved
            };

            var req2 = new MaintenanceRequest
            {
                Id = Guid.NewGuid(),
                PropertyId = prop2.Id,
                TenantId = tenant2.Id,
                Description = "A master bathroom shower faucet is slightly leaking under high pressure.",
                RequestDate = DateTime.UtcNow.AddDays(-10),
                Status = MaintenanceStatus.Open
            };

            context.MaintenanceRequests.AddRange(req1, req2);

            // 6. Seed Sample Applications
            var app1 = new Application
            {
                Id = Guid.NewGuid(),
                PropertyId = prop3.Id, // Bellevue Commercial Office (Available)
                TenantId = tenant1.Id, // John Smith
                Type = ApplicationType.Rental,
                Status = ApplicationStatus.Pending,
                AppliedDate = DateTime.UtcNow.AddDays(-5)
            };

            var app2 = new Application
            {
                Id = Guid.NewGuid(),
                PropertyId = prop4.Id, // Aspen Alpine Chalet (Available)
                TenantId = tenant2.Id, // Alice Cooper
                Type = ApplicationType.Rental,
                Status = ApplicationStatus.Pending,
                AppliedDate = DateTime.UtcNow.AddDays(-2)
            };

            context.Applications.AddRange(app1, app2);

            try
            {
                context.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                Console.WriteLine("=================================================");
                Console.WriteLine("❌ EF Core DbUpdateException:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine("=================================================");
                throw;
            }
        }
    }
}
