﻿using System;
using Hiroshima.DbData.Models.Accommodation;
using Hiroshima.DbData.Models.Booking;
using Hiroshima.DbData.Models.Rates;
using Hiroshima.DbData.Models.Rooms;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Location = Hiroshima.DbData.Models.Location;

namespace Hiroshima.DbData
{
    public class DirectContractsDbContext : DbContext
    {
        public DirectContractsDbContext(DbContextOptions<DirectContractsDbContext> options) : base(options)
        { }


        [DbFunction("st_distance_sphere")]
        public static double GetDistance(Point from, Point to)
            => throw new Exception();


        [DbFunction("jsonb_to_string")]
        public static string JsonbToString(string target)
            => throw new Exception();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis")
                .HasPostgresExtension("uuid-ossp");
            modelBuilder.UseIdentityColumns();
            AddRegions(modelBuilder);
            AddCountries(modelBuilder);
            AddLocalities(modelBuilder);
            AddLocation(modelBuilder);
            AddAccommodations(modelBuilder);
            AddRooms(modelBuilder);
            AddSeasons(modelBuilder);
            AddContractRates(modelBuilder);
            AddBookings(modelBuilder);
            AddStopSaleDate(modelBuilder);
            AddPermittedOccupancies(modelBuilder);
        }


        private void AddRegions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location.Region>(e =>
            {
                e.ToTable("Regions");
                e.HasKey(r => r.Id);
                e.Property(r => r.Name).HasColumnType("jsonb");
            });
        }


        private void AddCountries(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location.Country>(e =>
            {
                e.ToTable("Countries");
                e.HasKey(c => c.Code);
                e.Property(c => c.Code).IsRequired();
                e.Property(c => c.Name).HasColumnType("jsonb").IsRequired();
                e.HasOne(c => c.Region).WithMany(r => r.Countries)
                    .HasForeignKey(c => c.RegionId);
            });
        }


        private void AddLocalities(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("LocalityIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Location.Locality>(e =>
            {
                e.ToTable("Localities");
                e.HasKey(l => l.Id);
                e.Property(l => l.Id).HasDefaultValueSql("nextval('\"LocalityIdSeq\"')")
                    .IsRequired();
                e.Property(l => l.Name).HasColumnType("jsonb").IsRequired();
                e.HasOne(l => l.Country)
                    .WithMany(c => c.Localities).HasForeignKey(l => l.CountryCode);
            });
        }


        private void AddLocation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location.Location>(e =>
            {
                e.ToTable("Locations");
                modelBuilder.HasSequence<int>("LocationIdSeq").StartsAt(1).IncrementsBy(1);
                e.HasKey(l => l.Id);
                e.Property(l => l.Id).HasDefaultValueSql("nextval('\"LocationIdSeq\"')")
                    .IsRequired();
                e.Property(l => l.Coordinates).HasColumnType("geometry (point)")
                    .IsRequired();
                e.Property(l => l.Address).HasColumnType("jsonb").IsRequired();
                e.HasOne(l => l.Locality)
                    .WithMany(l => l.Locations).HasForeignKey(l => l.LocalityId);
            });
        }


        private void AddAccommodations(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("AccommodationIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Accommodation>(e =>
            {
                e.ToTable("Accommodations");
                e.HasKey(a => a.Id);
                e.Property(a => a.Id).HasDefaultValueSql("nextval('\"AccommodationIdSeq\"')")
                    .IsRequired();
                e.Property(a => a.Name).IsRequired().HasColumnType("jsonb").IsRequired();
                e.Property(a => a.TextualDescription).HasColumnType("jsonb").IsRequired();
                e.Property(a => a.Contacts).HasColumnType("jsonb");
                e.Property(a => a.Picture).HasColumnType("jsonb");
                e.Property(a => a.Schedule).HasColumnType("jsonb");
                e.Property(a => a.Rating);
                e.Property(a => a.PropertyType);
                e.Property(a => a.Amenities).HasColumnType("jsonb");
                e.Property(a => a.AdditionalInfo).HasColumnType("jsonb");
                e.Property(a => a.Features).HasColumnType("jsonb");
                e.HasOne(a => a.Location).WithOne(l => l.Accommodation)
                    .HasForeignKey<Location.Location>(l => l.AccommodationId);
            });

        }


        private void AddRooms(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("RoomsIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Room>(e =>
            {
                e.ToTable("Rooms");
                e.HasKey(r => r.Id);
                e.Property(r => r.Id).HasDefaultValueSql("nextval('\"RoomsIdSeq\"')")
                    .IsRequired();
                e.Property(r => r.Name).HasColumnType("jsonb");
                e.Property(r => r.Description).HasColumnType("jsonb");
                e.Property(r => r.Amenities).HasColumnType("jsonb");

                e.HasOne(r => r.Accommodation).WithMany(a => a.Rooms)
                    .HasForeignKey(r => r.AccommodationId);
            });
        }


        private void AddSeasons(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("SeasonIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Season>(e =>
            {
                e.ToTable("Seasons");
                e.HasKey(s => s.Id);
                e.Property(r => r.Id).HasDefaultValueSql("nextval('\"SeasonIdSeq\"')")
                    .IsRequired();
                e.Property(s => s.Name).IsRequired();
                e.Property(s => s.StartDate).IsRequired();
                e.Property(s => s.EndDate).IsRequired();
                e.HasOne(s => s.Accommodation).WithMany(a => a.Seasons)
                    .HasForeignKey(s => s.AccommodationId);
            });
        }


        private void AddContractRates(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("RateIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<ContractRate>(e =>
            {
                e.ToTable("ContractRates");
                e.HasKey(a => a.Id);
                e.Property(r => r.Id).HasDefaultValueSql("nextval('\"RateIdSeq\"')")
                    .IsRequired();
                e.Property(a => a.SeasonPrice).HasColumnType("numeric");
                e.Property(a => a.ExtraPersonPrice).HasColumnType("numeric");
                e.Property(a => a.CurrencyCode);
                e.Property(a => a.BoardBasisCode);
                e.Property(a => a.ReleaseDays);
                e.HasOne(a => a.Season).WithMany(a => a.ContractRates)
                    .HasForeignKey(a => a.SeasonId);
                e.HasOne(a => a.Room).WithMany(r => r.ContractRates)
                    .HasForeignKey(r => r.RoomId);
            });
        }


        private void AddBookings(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("BookingIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Booking>(e =>
            {
                e.ToTable("Booking");
                e.HasKey(b => b.Id);
                e.Property(b => b.Id).HasDefaultValueSql("nextval('\"BookingIdSeq\"')").IsRequired();
                e.Property(b => b.CreatedAt).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();
                e.Property(b => b.CheckInAt);
                e.Property(b => b.CheckOutAt);
                e.Property(b => b.BookedAt);
                e.Property(b => b.StatusCode);
                e.Property(b => b.LeadPassengerName);
                e.Property(b => b.Nationality);
                e.Property(b => b.Residency);
                e.HasOne(b => b.ContractRate).WithMany(a => a.Bookings)
                    .HasForeignKey(b => b.ContractRateId);
            });
        }


        private void AddStopSaleDate(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("StopSaleDateIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<StopSaleDate>(e =>
            {
                e.ToTable("StopSaleDates");
                e.HasKey(b => b.Id);
                e.Property(b => b.Id).HasDefaultValueSql("nextval('\"StopSaleDateIdSeq\"')").IsRequired();
                e.Property(b => b.StartDate).IsRequired();
                e.Property(b => b.EndDate).IsRequired();
                e.HasOne(b => b.Room)
                    .WithMany(a => a.StopSaleDates)
                    .HasForeignKey(b => b.RoomId);
            });
        }


        private void AddPermittedOccupancies(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("PermittedOccupancyIdSeq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<PermittedOccupancy>(e =>
            {
                e.ToTable("PermittedOccupancies");
                e.HasKey(p => p.Id);
                e.Property(b => b.Id).HasDefaultValueSql("nextval('\"PermittedOccupancyIdSeq\"')").IsRequired();
                e.Property(p => p.AdultsNumber).HasDefaultValue(0);
                e.Property(p => p.ChildrenNumber).HasDefaultValue(0);
                e.HasOne(p => p.Room).WithMany(a => a.PermittedOccupancies)
                    .HasForeignKey(p => p.RoomId);
            });
        }


        public virtual DbSet<Location.Locality> Localities { get; set; }
        public virtual DbSet<Location.Country> Countries { get; set; }
        public virtual DbSet<Location.Region> Regions { get; set; }
        public virtual DbSet<Location.Location> Locations { get; set; }
        public virtual DbSet<ContractRate> Rates { get; set; }
        public virtual DbSet<Accommodation> Accommodations { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Season> Seasons { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<PermittedOccupancy> PermittedOccupancies { get; set; }
        public virtual DbSet<StopSaleDate> StopSaleDates { get; set; }

    }

}
