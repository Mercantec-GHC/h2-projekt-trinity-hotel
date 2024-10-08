﻿using DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Data
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> dbContextOptions) : base(dbContextOptions)
        { }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PhoneNr).HasMaxLength(20);



                // Room configuration
                modelBuilder.Entity<Room>(entity =>
                {
                    entity.ToTable("Rooms");
                    entity.HasKey(r => r.RoomId);
                    entity.Property(r => r.Type).IsRequired();
                    entity.Property(r => r.Price).IsRequired();



                });

                // Booking configuration
                modelBuilder.Entity<Booking>(entity =>
                {
                    entity.ToTable("Bookings");
                    entity.HasKey(b => b.BookingId);
                    entity.Property(b => b.StartDate).IsRequired();
                    entity.Property(b => b.EndDate).IsRequired();

                });

                // Feedback configuration
                modelBuilder.Entity<Feedback>(entity =>
                {
                    entity.ToTable("Feedbacks");
                    entity.HasKey(f => f.FeedBackId);
                    entity.Property(f => f.FeedbackText).IsRequired();
                });
            });

        }
    }
}