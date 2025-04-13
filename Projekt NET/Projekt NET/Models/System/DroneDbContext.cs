using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
