using Microsoft.EntityFrameworkCore;
using TrackingServices.Domain.Entities;

namespace TrackingServices.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DriverLocation> DriverLocations => Set<DriverLocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DriverLocation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.CurrentCity).IsRequired().HasMaxLength(50);
                entity.Property(d => d.StatusMessage).HasMaxLength(200);
            });
        }
    }
}
