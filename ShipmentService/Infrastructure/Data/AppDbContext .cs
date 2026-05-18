using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Entities.Events;

namespace ShipmentService.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentEvent> ShipmentEvents => Set<ShipmentEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.TrackingNumber).IsUnique();
                entity.Property(s => s.TrackingNumber).IsRequired().HasMaxLength(20);
                entity.Property(s => s.PickupCity).IsRequired().HasMaxLength(50);
                entity.Property(s => s.DeliveryCity).IsRequired().HasMaxLength(50);
                entity.Property(s => s.CurrentStatus).HasConversion<int>();
            });

            modelBuilder.Entity<ShipmentEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(50);
            });
        }
    }
}
