﻿using System;
using System.Text.Json;
using HappyTravel.Hiroshima.Data.Models;
using HappyTravel.Hiroshima.Data.Models.Accommodations;
using HappyTravel.Hiroshima.Data.Models.Booking;
using HappyTravel.Hiroshima.Data.Models.Location;
using HappyTravel.Hiroshima.Data.Models.Rooms;
using HappyTravel.Hiroshima.Data.Models.Rooms.CancellationPolicies;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Room = HappyTravel.Hiroshima.Data.Models.Rooms.Room;

namespace HappyTravel.Hiroshima.Data
{
    public class DirectContractsDbContext : DbContext
    {
        public DirectContractsDbContext(DbContextOptions<DirectContractsDbContext> options) : base(options)
        {}


        [DbFunction("st_distance_sphere")]
        public static double GetDistance(Point from, Point to)
            => throw new Exception();

        
        [DbFunction("lang_from_jsonb")]
        public static JsonDocument GetLangFromJsonb(JsonDocument jsonb, string languageCode)
            => throw new Exception();
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis")
                .HasPostgresExtension("uuid-ossp");
            modelBuilder.UseIdentityColumns();

            AddContractManagers(modelBuilder);
            AddContracts(modelBuilder);
            AddLocations(modelBuilder);
            AddAccommodations(modelBuilder);
            AddRooms(modelBuilder);
            AddRates(modelBuilder);
            AddRoomAvailabilityRestrictions(modelBuilder);
            AddPromotionalOffers(modelBuilder);
            AddRoomAllocationRequirements(modelBuilder);
            AddBooking(modelBuilder);
            AddCountries(modelBuilder);
            AddCancellationPolicies(modelBuilder);
            AddContractAccommodationRelation(modelBuilder);
        }


        private void AddContractManagers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContractManager>(e =>
            {
                e.ToTable("ContractManagers");
                e.HasKey(c => c.Id);
                e.Property(c => c.IdentityHash);
                e.Property(c => c.Email).IsRequired();
                e.Property(c => c.Name).IsRequired();
                e.Property(c => c.Title);
            });
        }
        
        
        private void AddContracts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>(e =>
            {
                e.ToTable("Contracts");
                e.HasKey(c => c.Id);
                e.Property(c => c.ValidFrom).IsRequired();
                e.Property(c => c.ValidTo).IsRequired();
                e.Property(c => c.Name).IsRequired();
                e.Property(c => c.Description);
                e.Property(c => c.ContractManagerId).IsRequired();
                e.HasIndex(c => c.ContractManagerId);
            });
        }
        
        
        private void AddLocations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Location.Location>(e =>
            {
                e.ToTable("Locations");
                e.HasKey(l => l.Id);
                e.Property(l => l.Locality ).HasColumnType("jsonb").IsRequired();
                e.Property(l => l.Zone).HasColumnType("jsonb");
                e.Property(l => l.CountryCode).IsRequired();
                e.HasKey(l => l.CountryCode);
            });
        }


        private void AddCountries(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(e =>
            {
                e.ToTable("Countries");
                e.HasKey(c => c.Code);
                e.Property(c => c.Name).HasColumnType("jsonb");
            });
        }
        

        private void AddAccommodations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accommodation>(e =>
            {
                e.ToTable("Accommodations");
                e.HasKey(a => a.Id);
                e.Property(a => a.Address).HasColumnType("jsonb").IsRequired();
                e.Property(a=> a.ContactInfo).HasColumnType("jsonb").IsRequired();
                e.Property(a => a.Coordinates).HasColumnType("geometry (point)").IsRequired();
                e.Property(a=> a.Name).HasColumnType("jsonb").IsRequired();
                e.Property(a => a.Pictures).HasColumnType("jsonb");
                e.Property(a => a.Rating).IsRequired();
                e.Property(a=> a.AccommodationAmenities).HasColumnType("jsonb");
                e.Property(a=> a.AdditionalInfo).HasColumnType("jsonb");
                e.Property(a => a.PropertyType);
                e.Property(a => a.TextualDescription).HasColumnType("jsonb");
                e.Property(a => a.CheckInTime);
                e.Property(a => a.CheckOutTime);
                e.Property(a => a.OccupancyDefinition).HasColumnType("jsonb");
                e.Property(a => a.ContractManagerId).IsRequired();
                e.HasIndex(a=> a.Coordinates).HasMethod("GIST");
                e.HasIndex(a => a.LocationId);
                e.HasIndex(a => a.ContractManagerId);
            });
        }

        
        private void AddRooms(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>(e =>
            {
                e.ToTable("Rooms");
                e.HasKey(r=> r.Id);
                e.Property(r=> r.Amenities).HasColumnType("jsonb");
                e.Property(r => r.Description).HasColumnType("jsonb");
                e.Property(r => r.Name).HasColumnType("jsonb").IsRequired();
                e.Property(r => r.OccupancyConfigurations).HasColumnType("jsonb").IsRequired();
                e.HasOne<Accommodation>().WithMany().HasForeignKey(r => r.AccommodationId).IsRequired();
            });
        }

        
        private void AddRates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomRate>(e =>
            {
                e.ToTable("RoomRates");
                e.HasKey(rr=> rr.Id);
                e.Property(rr => rr.Price).IsRequired();
                e.Property(rr => rr.CurrencyCode).IsRequired();
                e.Property(rr => rr.MealPlan);
                e.Property(rr => rr.BoardBasis);
                e.Property(rr=> rr.StartDate).IsRequired();
                e.Property(rr=> rr.EndDate).IsRequired();
                e.HasOne<Room>().WithMany().HasForeignKey(rr => rr.RoomId);
            });
        }

        
        private void AddRoomAvailabilityRestrictions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomAvailabilityRestrictions>(e =>
            {
                e.ToTable("RoomAvailabilityRestrictions");
                e.HasKey(rr => rr.Id);
                e.Property(rr => rr.Restrictions).IsRequired().HasDefaultValue(SaleRestrictions.StopSale);
                e.Property(rr => rr.StartDate).IsRequired();
                e.Property(rr => rr.EndDate).IsRequired();
                e.HasOne<Room>().WithMany().HasForeignKey(rr => rr.RoomId);
            });
        }


        private void AddPromotionalOffers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomPromotionalOffer>(e =>
            {
                e.ToTable("RoomPromotionalOffers");
                e.HasKey(po => po.Id);
                e.Property(po => po.DiscountPercent).IsRequired();
                e.Property(po => po.Details).HasColumnType("jsonb");
                e.Property(po => po.BookByDate);
                e.Property(po => po.ValidFromDate).IsRequired();
                e.Property(po => po.ValidToDate).IsRequired();
                e.Property(po => po.BookingCode).IsRequired();
                e.HasOne<Room>().WithMany().HasForeignKey(po => po.RoomId);
            });
        }


        private void AddRoomAllocationRequirements(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomAllocationRequirement>(e =>
            {
                e.ToTable("RoomAllocationRequirements");
                e.HasKey(rar => rar.Id);
                e.Property(rar => rar.StartDate).IsRequired();
                e.Property(rar => rar.EndDate).IsRequired();
                e.Property(rar => rar.MinimumStayNights);
                e.Property(rar => rar.ReleasePeriod).HasColumnType("jsonb");
                e.Property(rar => rar.Allotment);
                e.HasOne<Room>().WithMany().HasForeignKey(rar => rar.RoomId);
            });
        }
        
        
        private void AddBooking(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(e =>
            {
                e.ToTable("BookingOrders");
                e.HasKey(bo => bo.Id);
                e.Property(bo => bo.ReferenceCode).IsRequired();
                e.Property(bo => bo.StatusCode).IsRequired();
                e.Property(bo => bo.BookingDate).IsRequired();
                e.Property(bo => bo.CheckInDate).IsRequired();
                e.Property(bo => bo.CheckOutDate).IsRequired();
                e.Property(bo => bo.Rooms).HasColumnType("jsonb").IsRequired();
                e.Property(bo => bo.Nationality).IsRequired();
                e.Property(bo => bo.Residency).IsRequired();
                e.Property(bo => bo.LanguageCode).IsRequired();
            });
        }

        
        private void AddCancellationPolicies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomCancellationPolicy>(e =>
            {
                e.ToTable("CancellationPolicies");
                e.HasKey(cp => cp.Id);
                e.Property(cp => cp.StartDate).IsRequired();
                e.Property(cp => cp.EndDate).IsRequired();
                e.Property(bo => bo.Details).HasColumnType("jsonb").IsRequired();
                e.Property(bo => bo.RoomId).IsRequired();
            });
        }


        private void AddContractAccommodationRelation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContractAccommodationRelation>(e =>
            {
                e.ToTable("ContractAccommodationRelations");
                e.HasKey(car => car.Id);
                e.Property(car => car.AccommodationId).IsRequired();
                e.Property(car => car.ContractId).IsRequired();
                e.HasOne<Contract>().WithMany().HasForeignKey(rar => rar.ContractId);
                e.HasOne<Accommodation>().WithMany().HasForeignKey(rar => rar.AccommodationId);
            });
        }
        
        
        public virtual DbSet<Models.Location.Location> Locations { get; set; }
        public virtual DbSet<Accommodation> Accommodations { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomAvailabilityRestrictions> RoomAvailabilityRestrictions { get; set; }
        public virtual DbSet<RoomRate> RoomRates { get; set; }
        public virtual DbSet<RoomAllocationRequirement> RoomAllocationRequirements { get; set; }
        public virtual DbSet<RoomPromotionalOffer> RoomPromotionalOffers { get; set; }
        public virtual DbSet<Booking> Booking { get; set; }
        public virtual DbSet<RoomCancellationPolicy> CancellationPolicies { get; set; }
        public virtual DbSet<ContractManager> ContractManagers { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<ContractAccommodationRelation> ContractAccommodationRelations { get; set; }
    }
}