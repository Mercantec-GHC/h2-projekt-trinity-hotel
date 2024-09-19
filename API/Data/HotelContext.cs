using DomainModels;
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

                // Relationship with Booking
                entity.HasMany(u => u.RoomBookings)
                      .WithOne(b => b.User)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Room configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Rooms");
                entity.HasKey(r => r.RoomId);
                entity.Property(r => r.Type).IsRequired();
                entity.Property(r => r.Price).IsRequired();
                entity.Property(r => r.BookedDays).HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList()
                );

                // Add value comparers for BookedDays
                entity.Property(r => r.BookedDays)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList())
                      .Metadata.SetValueComparer(new ValueComparer<List<DateTime>>(
                          (c1, c2) => c1.SequenceEqual(c2),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToList()));

                // Relationship with Booking
                entity.HasMany(r => r.Bookings)
                      .WithOne(b => b.Room)
                      .HasForeignKey(b => b.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(b => b.BookingId);
                entity.Property(b => b.FullName).IsRequired();
                entity.Property(b => b.Email).IsRequired();
                entity.Property(b => b.StartDate).IsRequired();
                entity.Property(b => b.EndDate).IsRequired();
                entity.Property(b => b.BookedDays).HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList()
                );

                // Add value comparers for BookedDays
                entity.Property(b => b.BookedDays)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList())
                      .Metadata.SetValueComparer(new ValueComparer<List<DateTime>>(
                          (c1, c2) => c1.SequenceEqual(c2),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToList()));

                // Relationship with Room
                entity.HasOne(b => b.Room)
                    .WithMany(r => r.Bookings)
                    .HasForeignKey(b => b.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with User
                entity.HasOne(b => b.User)
                    .WithMany(u => u.RoomBookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Feedback configuration
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedbacks");
                entity.HasKey(f => f.FeedBackId);
                entity.Property(f => f.FeedbackText).IsRequired();
            });
        }

    }
}