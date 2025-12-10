using Domain.Entities.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Guide> Guides { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourImage> TourImages { get; set; }
        public DbSet<TourItinerary> TourItineraries { get; set; }
        public DbSet<TourInclude> TourIncludes { get; set; }
        public DbSet<TourExclude> TourExcludes { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TourTag> TourTags { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingGuest> BookingGuests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<GuideReview> GuideReviews { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DailyStatistic> DailyStatistics { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TourDeparture> TourDepartures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================================
            // 1. USER & AUTHENTICATION
            // ===================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Avatar).HasMaxLength(500);

                // Indexes
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.Status);

                // Query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
                entity.Property(e => e.DeviceInfo).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 2. GUIDE
            // ===================================

            modelBuilder.Entity<Guide>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email);
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.Bio).HasMaxLength(2000);
                entity.Property(e => e.Languages).HasMaxLength(500);
                entity.Property(e => e.AverageRating).HasPrecision(3, 2);

                // One-to-One with User (optional)
                entity.HasOne(e => e.User)
                    .WithOne(u => u.GuideProfile)
                    .HasForeignKey<Guide>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 3. DESTINATION
            // ===================================

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Country).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(250).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(5000);
                entity.Property(e => e.AverageRating).HasPrecision(3, 2);
                entity.Property(e => e.StartingPrice).HasPrecision(18, 2);
                entity.Property(e => e.MetaTitle).HasMaxLength(200);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 4. TOUR
            // ===================================

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(350).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Duration).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(10000).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.AverageRating).HasPrecision(3, 2);
                entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);
                entity.Property(e => e.PhysicalRequirements).HasMaxLength(1000);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(1000);
                entity.Property(e => e.MetaTitle).HasMaxLength(200);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);

                // Indexes
                entity.HasIndex(e => e.DestinationId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Type);

                // Relationships
                entity.HasOne(e => e.Destination)
                    .WithMany(d => d.Tours)
                    .HasForeignKey(e => e.DestinationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 5. TOUR RELATED ENTITIES
            // ===================================

            modelBuilder.Entity<TourImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Caption).HasMaxLength(500);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Images)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TourItinerary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(5000).IsRequired();
                entity.Property(e => e.Activities).HasMaxLength(2000);
                entity.Property(e => e.Meals).HasMaxLength(500);
                entity.Property(e => e.Accommodation).HasMaxLength(500);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Itineraries)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.TourId, e.DayNumber });
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TourInclude>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Item).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Includes)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TourExclude>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Item).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Excludes)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 6. TOUR-GUIDE (Many-to-Many)
            // ===================================

            modelBuilder.Entity<TourGuide>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.TourGuides)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Guide)
                    .WithMany(g => g.TourGuides)
                    .HasForeignKey(e => e.GuideId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Composite index
                entity.HasIndex(e => new { e.TourId, e.GuideId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 7. TAG & TOUR-TAG (Many-to-Many)
            // ===================================

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Icon).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TourTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.TourTags)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.TourTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.TourId, e.TagId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 8. TOUR DEPARTURE (NEW)
            // ===================================

            modelBuilder.Entity<TourDeparture>(entity =>
            {
                entity.ToTable("TourDepartures");
                entity.HasKey(td => td.Id);

                entity.Property(td => td.DepartureDate)
                    .IsRequired();

                entity.Property(td => td.EndDate)
                    .IsRequired();

                entity.Property(td => td.MaxGuests)
                    .IsRequired();

                entity.Property(td => td.BookedGuests)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(td => td.SpecialPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(td => td.Status)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(td => td.Notes)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(td => td.Tour)
                    .WithMany()
                    .HasForeignKey(td => td.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(td => td.DefaultGuide)
                    .WithMany()
                    .HasForeignKey(td => td.DefaultGuideId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(td => td.Bookings)
                    .WithOne(b => b.TourDeparture)
                    .HasForeignKey(b => b.TourDepartureId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(td => td.TourId)
                    .HasDatabaseName("IX_TourDepartures_TourId");

                entity.HasIndex(td => td.DepartureDate)
                    .HasDatabaseName("IX_TourDepartures_DepartureDate");

                entity.HasIndex(td => td.Status)
                    .HasDatabaseName("IX_TourDepartures_Status");

                entity.HasIndex(td => new { td.TourId, td.DepartureDate })
                    .HasDatabaseName("IX_TourDepartures_TourId_DepartureDate")
                    .HasFilter("[IsDeleted] = 0");

                // Query filter for soft delete
                entity.HasQueryFilter(td => !td.IsDeleted);
            });

            // ===================================
            // 9. BOOKING (UPDATED)
            // ===================================

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookingCode).IsUnique();
                entity.Property(e => e.BookingCode).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CustomerEmail).HasMaxLength(255).IsRequired();
                entity.Property(e => e.CustomerPhone).HasMaxLength(20).IsRequired();
                entity.Property(e => e.SpecialRequests).HasMaxLength(2000);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
                entity.Property(e => e.PaymentTransactionId).HasMaxLength(200);
                entity.Property(e => e.CancellationReason).HasMaxLength(1000);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TourId);
                entity.HasIndex(e => e.GuideId);
                entity.HasIndex(e => e.TourDepartureId); // NEW
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.TourDate);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Bookings)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Guide)
                    .WithMany(g => g.Bookings)
                    .HasForeignKey(e => e.GuideId)
                    .OnDelete(DeleteBehavior.SetNull);

                // NEW: TourDeparture relationship
                entity.HasOne(e => e.TourDeparture)
                    .WithMany(td => td.Bookings)
                    .HasForeignKey(e => e.TourDepartureId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<BookingGuest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Gender).HasMaxLength(20).IsRequired();
                entity.Property(e => e.PassportNumber).HasMaxLength(50);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(1000);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Guests)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 10. REVIEW (Tour Review)
            // ===================================

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.Comment).HasMaxLength(5000).IsRequired();

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TourId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Rating);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Reviews)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Booking)
                    .WithOne(b => b.TourReview)
                    .HasForeignKey<Review>(e => e.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.BookingId).IsUnique();

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ReviewImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.Review)
                    .WithMany(r => r.Images)
                    .HasForeignKey(e => e.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 11. GUIDE REVIEW
            // ===================================

            modelBuilder.Entity<GuideReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.Comment).HasMaxLength(5000).IsRequired();

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.GuideId);
                entity.HasIndex(e => e.BookingId).IsUnique(); // One booking = one guide review
                entity.HasIndex(e => e.Status);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.GuideReviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Guide)
                    .WithMany(g => g.GuideReviews)
                    .HasForeignKey(e => e.GuideId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Booking)
                    .WithOne(b => b.GuideReview)
                    .HasForeignKey<GuideReview>(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 12. FAVORITE
            // ===================================

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tour)
                    .WithMany(t => t.Favorites)
                    .HasForeignKey(e => e.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One user can only favorite a tour once
                entity.HasIndex(e => new { e.UserId, e.TourId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 13. NOTIFICATION
            // ===================================

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
                entity.Property(e => e.Link).HasMaxLength(500);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ===================================
            // 14. STATISTICS & AUDIT
            // ===================================

            modelBuilder.Entity<DailyStatistic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Date).IsUnique();
                entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===================================
            // SEED DATA
            // ===================================
            DatabaseSeeder.SeedData(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}