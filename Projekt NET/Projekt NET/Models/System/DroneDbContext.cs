using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models;

namespace Projekt_NET.Models.System
{
    public class DroneDbContext : DbContext
    {
        public DroneDbContext(DbContextOptions<DroneDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Drone> Drones { get; set; }
        public DbSet<DroneCloud> DroneClouds { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryLog> Deliverylogs { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightPath> FlightPaths { get; set; }
        public DbSet<Operator> Operators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>()
               .HasOne(f => f.Drone)
               .WithMany()
               .HasForeignKey(f => f.DroneId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Package>()
               .HasOne(p => p.Drone)
               .WithMany()
               .HasForeignKey(p => p.DroneId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Drone>()
            .OwnsOne(d => d.Coordinate);

            modelBuilder.Entity<District>()
                .OwnsMany(d => d.BoundingPoints);
        }
    }
}
