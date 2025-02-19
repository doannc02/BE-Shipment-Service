using Ichiba.Shipment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Infrastructure.Data
{
    public class ShipmentDbContext : DbContext
    {
        public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
       : base(options)
        {
        }

        public DbSet<ShipmentEntity> Shipments { get; set; }
        public DbSet<ShipmentAddress> ShipmentAddresses { get; set; }
        public DbSet<ShipmentPackage> ShipmentPackages { get; set; }
        public DbSet<Package> Packages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=logistics_system;Username=postgres;Password=123123");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  ShipmentEntity - ShipmentAddress
            modelBuilder.Entity<ShipmentEntity>()
                .HasMany(s => s.Addresses)
                .WithOne(a => a.Shipment)
                .HasForeignKey(a => a.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            //  ShipmentEntity - ShipmentPackage
            modelBuilder.Entity<ShipmentEntity>()
                .HasMany(s => s.ShipmentPackages)
                .WithOne(sp => sp.Shipment)
                .HasForeignKey(sp => sp.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            //  ShipmentPackage - Package
            modelBuilder.Entity<ShipmentPackage>()
                .HasOne(sp => sp.Package)
                .WithMany()
                .HasForeignKey(sp => sp.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
