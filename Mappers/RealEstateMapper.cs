using System;
using RealEstate.Models;
using RealEstate.Models.DTOs.Property;
using RealEstate.Models.DTOs.Person;
using RealEstate.Models.DTOs.Lease;
using RealEstate.Models.DTOs.Payment;
using RealEstate.Models.DTOs.Maintenance;
using RealEstate.Models.DTOs.Application;
using RealEstate.Models.Enums;

namespace RealEstate.Mappers
{
    public static class RealEstateMapper
    {
        // ==========================================
        // PROPERTY MAPPINGS
        // ==========================================
        public static PropertyDto ToPropertyDto(this Property property)
        {
            return new PropertyDto
            {
                Id = property.Id,
                Address = property.Address,
                City = property.City,
                Province = property.Province,
                Type = property.Type,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                MonthlyRent = property.MonthlyRent,
                Status = property.Status,
                ListingType = property.ListingType,
                LandlordId = property.LandlordId,
                ImageUrls = property.ImageUrls,
                Description = property.Description,
                Size = property.Size
            };
        }

        public static Property ToPropertyFromCreate(this CreatePropertyDto dto)
        {
            return new Property
            {
                Id = Guid.NewGuid(),
                Address = dto.Address,
                City = dto.City,
                Province = dto.Province,
                Type = dto.Type,
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                MonthlyRent = dto.MonthlyRent,
                Status = PropertyStatus.Available, // Default to Available upon creation
                ListingType = dto.ListingType,
                LandlordId = dto.LandlordId,
                ImageUrls = dto.ImageUrls,
                Description = dto.Description,
                Size = dto.Size
            };
        }

        public static void UpdateProperty(this Property property, UpdatePropertyDto dto)
        {
            if (dto.Address != null) property.Address = dto.Address;
            if (dto.City != null) property.City = dto.City;
            if (dto.Province != null) property.Province = dto.Province;
            if (dto.Type != null) property.Type = dto.Type.Value;
            if (dto.Bedrooms != null) property.Bedrooms = dto.Bedrooms.Value;
            if (dto.Bathrooms != null) property.Bathrooms = dto.Bathrooms.Value;
            if (dto.MonthlyRent != null) property.MonthlyRent = dto.MonthlyRent.Value;
            if (dto.Status != null) property.Status = dto.Status.Value;
            if (dto.ListingType != null) property.ListingType = dto.ListingType.Value;
            if (dto.Description != null) property.Description = dto.Description;
            if (dto.Size != null) property.Size = dto.Size.Value;
        }

        // ==========================================
        // PERSON MAPPINGS
        // ==========================================
        public static PersonDto ToPersonDto(this Person person)
        {
            return new PersonDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber,
                Role = person.Role
            };
        }

        public static Person ToPersonFromCreate(this CreatePersonDto dto)
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role
            };
        }

        public static void UpdatePerson(this Person person, UpdatePersonDto dto)
        {
            if (dto.FirstName != null) person.FirstName = dto.FirstName;
            if (dto.LastName != null) person.LastName = dto.LastName;
            if (dto.Email != null) person.Email = dto.Email;
            if (dto.PhoneNumber != null) person.PhoneNumber = dto.PhoneNumber;
            if (dto.Role != null) person.Role = dto.Role.Value;
        }

        // ==========================================
        // LEASE MAPPINGS
        // ==========================================
        public static LeaseDto ToLeaseDto(this Lease lease)
        {
            return new LeaseDto
            {
                Id = lease.Id,
                PropertyId = lease.PropertyId,
                TenantId = lease.TenantId,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                MonthlyRent = lease.MonthlyRent,
                SecurityDeposit = lease.SecurityDeposit,
                IsActive = lease.IsActive
            };
        }

        public static Lease ToLeaseFromCreate(this CreateLeaseDto dto)
        {
            return new Lease
            {
                Id = Guid.NewGuid(),
                PropertyId = dto.PropertyId,
                TenantId = dto.TenantId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyRent = dto.MonthlyRent,
                SecurityDeposit = dto.SecurityDeposit,
                IsActive = true // Active by default
            };
        }

        public static void UpdateLease(this Lease lease, UpdateLeaseDto dto)
        {
            if (dto.StartDate != null) lease.StartDate = dto.StartDate.Value;
            if (dto.EndDate != null) lease.EndDate = dto.EndDate.Value;
            if (dto.MonthlyRent != null) lease.MonthlyRent = dto.MonthlyRent.Value;
            if (dto.SecurityDeposit != null) lease.SecurityDeposit = dto.SecurityDeposit.Value;
            if (dto.IsActive != null) lease.IsActive = dto.IsActive.Value;
        }

        // ==========================================
        // PAYMENT MAPPINGS
        // ==========================================
        public static PaymentDto ToPaymentDto(this Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                LeaseId = payment.LeaseId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Type = payment.Type,
                Status = payment.Status
            };
        }

        public static Payment ToPaymentFromCreate(this CreatePaymentDto dto)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                LeaseId = dto.LeaseId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                Type = dto.Type,
                Status = PaymentStatus.Pending // Pending by default
            };
        }

        public static void UpdatePayment(this Payment payment, UpdatePaymentDto dto)
        {
            if (dto.Status != null) payment.Status = dto.Status.Value;
        }

        // ==========================================
        // MAINTENANCE REQUEST MAPPINGS
        // ==========================================
        public static MaintenanceRequestDto ToMaintenanceRequestDto(this MaintenanceRequest request)
        {
            return new MaintenanceRequestDto
            {
                Id = request.Id,
                PropertyId = request.PropertyId,
                TenantId = request.TenantId,
                Description = request.Description,
                RequestDate = request.RequestDate,
                Status = request.Status
            };
        }

        public static MaintenanceRequest ToMaintenanceRequestFromCreate(this CreateMaintenanceRequestDto dto)
        {
            return new MaintenanceRequest
            {
                Id = Guid.NewGuid(),
                PropertyId = dto.PropertyId,
                TenantId = dto.TenantId,
                Description = dto.Description,
                RequestDate = DateTime.UtcNow,
                Status = MaintenanceStatus.Open // Open by default
            };
        }

        public static void UpdateMaintenanceRequest(this MaintenanceRequest request, UpdateMaintenanceRequestDto dto)
        {
            if (dto.Description != null) request.Description = dto.Description;
            if (dto.Status != null) request.Status = dto.Status.Value;
        }

        // ==========================================
        // APPLICATION MAPPINGS
        // ==========================================
        public static ApplicationDto ToApplicationDto(this Application application)
        {
            return new ApplicationDto
            {
                Id = application.Id,
                PropertyId = application.PropertyId,
                TenantId = application.TenantId,
                Type = application.Type,
                Status = application.Status,
                AppliedDate = application.AppliedDate
            };
        }

        public static Application ToApplicationFromCreate(this CreateApplicationDto dto)
        {
            return new Application
            {
                Id = Guid.NewGuid(),
                PropertyId = dto.PropertyId,
                TenantId = dto.TenantId,
                Type = dto.Type,
                Status = ApplicationStatus.Pending, // Pending by default
                AppliedDate = DateTime.UtcNow
            };
        }
    }
}
