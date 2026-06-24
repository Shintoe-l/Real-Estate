using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Property Relationships & Constraints
            modelBuilder.Entity<Property>()
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(p => p.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Property>()
                .Property(p => p.MonthlyRent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Property>()
                .Property(p => p.Bathrooms)
                .HasColumnType("decimal(3,1)"); // e.g. 2.5 bathrooms

            // Configure Lease Relationships & Constraints
            modelBuilder.Entity<Lease>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(l => l.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lease>()
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lease>()
                .Property(l => l.MonthlyRent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lease>()
                .Property(l => l.SecurityDeposit)
                .HasColumnType("decimal(18,2)");

            // Configure Payment Relationships & Constraints
            modelBuilder.Entity<Payment>()
                .HasOne<Lease>()
                .WithMany()
                .HasForeignKey(p => p.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // Configure MaintenanceRequest Relationships & Constraints
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Application Relationships & Constraints
            modelBuilder.Entity<Application>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(a => a.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
