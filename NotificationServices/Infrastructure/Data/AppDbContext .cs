using Microsoft.EntityFrameworkCore;
using NotificationServices.Domain.Entities;

namespace NotificationServices.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(100);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
            });
        }
    }
}
