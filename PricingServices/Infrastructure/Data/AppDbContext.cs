using Microsoft.EntityFrameworkCore;
using PricingServices.Domain.Entities;

namespace PricingServices.Infrastructure.Data
{
    

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Invoice> Invoices => Set<Invoice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
                entity.HasIndex(i => i.ShipmentId).IsUnique();
                entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
                entity.Property(i => i.PickupCity).IsRequired().HasMaxLength(50);
                entity.Property(i => i.DeliveryCity).IsRequired().HasMaxLength(50);
                entity.Property(i => i.Status).HasConversion<int>();
            });
        }
    }
}
