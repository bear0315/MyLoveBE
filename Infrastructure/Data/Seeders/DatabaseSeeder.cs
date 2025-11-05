using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(ModelBuilder modelBuilder)
        {
            // ===================================
            // 1. USERS
            // ===================================
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Email = "admin@tourapp.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f", // Password123!
                    FullName = "System Administrator",
                    PhoneNumber = "+84901234567",
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2023, 1, 1),
                    LastLoginAt = new DateTime(2024, 11, 5, 10, 0, 0),
                    CreatedAt = new DateTime(2023, 1, 1),
                    IsDeleted = false
                },
                new User
                {
                    Id = 2,
                    Email = "manager@tourapp.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "Tour Manager",
                    PhoneNumber = "+84901234568",
                    Role = UserRole.Manager,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2023, 2, 1),
                    LastLoginAt = new DateTime(2024, 11, 4, 10, 0, 0),
                    CreatedAt = new DateTime(2023, 2, 1),
                    IsDeleted = false
                },
                new User
                {
                    Id = 3,
                    Email = "john.guide@tourapp.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "John Smith",
                    PhoneNumber = "+84901234569",
                    Role = UserRole.Guide,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2023, 3, 1),
                    LastLoginAt = new DateTime(2024, 11, 5, 8, 0, 0),
                    CreatedAt = new DateTime(2023, 3, 1),
                    IsDeleted = false
                },
                new User
                {
                    Id = 4,
                    Email = "nguyen.guide@tourapp.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "Nguyen Van An",
                    PhoneNumber = "+84901234570",
                    Role = UserRole.Guide,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2023, 3, 15),
                    LastLoginAt = new DateTime(2024, 11, 5, 5, 0, 0),
                    CreatedAt = new DateTime(2023, 3, 15),
                    IsDeleted = false
                },
                new User
                {
                    Id = 5,
                    Email = "customer1@email.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "David Johnson",
                    PhoneNumber = "+1234567890",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2024, 1, 15),
                    LastLoginAt = new DateTime(2024, 11, 2, 10, 0, 0),
                    CreatedAt = new DateTime(2024, 1, 15),
                    IsDeleted = false
                },
                new User
                {
                    Id = 6,
                    Email = "customer2@email.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "Sarah Williams",
                    PhoneNumber = "+1234567891",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2024, 2, 20),
                    LastLoginAt = new DateTime(2024, 11, 4, 10, 0, 0),
                    CreatedAt = new DateTime(2024, 2, 20),
                    IsDeleted = false
                },
                new User
                {
                    Id = 7,
                    Email = "customer3@email.com",
                    PasswordHash = "$2a$11$XKXHGkzJfYfqf.n6fJ5gxuK9VW.9Jfx5YzLXxJxP5QxfN5fJ5fJ5f",
                    FullName = "Tran Thi Mai",
                    PhoneNumber = "+84901234571",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active,
                    MemberSince = new DateTime(2024, 3, 10),
                    LastLoginAt = new DateTime(2024, 11, 5, 4, 0, 0),
                    CreatedAt = new DateTime(2024, 3, 10),
                    IsDeleted = false
                }
            };

            modelBuilder.Entity<User>().HasData(users);

            // ===================================
            // 2. GUIDES
            // ===================================
            var guides = new List<Guide>
            {
                new Guide
                {
                    Id = 1,
                    UserId = 3,
                    FullName = "John Smith",
                    Email = "john.guide@tourapp.com",
                    PhoneNumber = "+84901234569",
                    Avatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d",
                    Bio = "Experienced tour guide with 10 years in the industry. Specialized in cultural and adventure tours across Vietnam.",
                    Languages = "English, Vietnamese, French",
                    ExperienceYears = 10,
                    AverageRating = 4.8m,
                    TotalReviews = 127,
                    Status = GuideStatus.Active,
                    CreatedAt = new DateTime(2023, 3, 1),
                    IsDeleted = false
                },
                new Guide
                {
                    Id = 2,
                    UserId = 4,
                    FullName = "Nguyen Van An",
                    Email = "nguyen.guide@tourapp.com",
                    PhoneNumber = "+84901234570",
                    Avatar = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d",
                    Bio = "Local expert with deep knowledge of Vietnamese history and culture. Passionate about sharing hidden gems.",
                    Languages = "Vietnamese, English, Chinese",
                    ExperienceYears = 7,
                    AverageRating = 4.9m,
                    TotalReviews = 95,
                    Status = GuideStatus.Active,
                    CreatedAt = new DateTime(2023, 3, 15),
                    IsDeleted = false
                },
                new Guide
                {
                    Id = 3,
                    UserId = null,
                    FullName = "Maria Garcia",
                    Email = "maria.garcia@tourapp.com",
                    PhoneNumber = "+84901234572",
                    Avatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330",
                    Bio = "Multilingual guide specializing in culinary tours and cooking experiences in Hoi An.",
                    Languages = "Spanish, English, Vietnamese",
                    ExperienceYears = 5,
                    AverageRating = 4.7m,
                    TotalReviews = 68,
                    Status = GuideStatus.Active,
                    CreatedAt = new DateTime(2023, 6, 1),
                    IsDeleted = false
                }
            };

            modelBuilder.Entity<Guide>().HasData(guides);

            // ===================================
            // 3. DESTINATIONS
            // ===================================
            var destinations = new List<Destination>
            {
                new Destination
                {
                    Id = 1,
                    Name = "Ha Long Bay",
                    Country = "Vietnam",
                    Description = "A UNESCO World Heritage Site featuring thousands of limestone karsts and islands in emerald waters.",
                    ImageUrl = "https://images.unsplash.com/photo-1528127269322-539801943592",
                    AverageRating = 4.7m,
                    TotalReviews = 2145,
                    StartingPrice = 1200000m,
                    TourCount = 15,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    Slug = "ha-long-bay",
                    MetaTitle = "Ha Long Bay Tours - UNESCO Heritage Site",
                    MetaDescription = "Explore the breathtaking Ha Long Bay with luxury cruises and adventure tours.",
                    CreatedAt = new DateTime(2023, 1, 15),
                    IsDeleted = false
                },
                new Destination
                {
                    Id = 2,
                    Name = "Hoi An Ancient Town",
                    Country = "Vietnam",
                    Description = "A well-preserved ancient town showcasing a blend of Vietnamese, Chinese, Japanese, and European architecture.",
                    ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482",
                    AverageRating = 4.8m,
                    TotalReviews = 1890,
                    StartingPrice = 800000m,
                    TourCount = 20,
                    IsFeatured = true,
                    DisplayOrder = 2,
                    Slug = "hoi-an-ancient-town",
                    MetaTitle = "Hoi An Tours - Ancient Town & Lantern Festival",
                    MetaDescription = "Discover the charming streets of Hoi An, famous for lanterns, tailors, and cultural heritage.",
                    CreatedAt = new DateTime(2023, 1, 15),
                    IsDeleted = false
                },
                new Destination
                {
                    Id = 3,
                    Name = "Sapa",
                    Country = "Vietnam",
                    Description = "Mountain town known for stunning rice terraces, ethnic minority villages, and trekking adventures.",
                    ImageUrl = "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b",
                    AverageRating = 4.6m,
                    TotalReviews = 1234,
                    StartingPrice = 1500000m,
                    TourCount = 12,
                    IsFeatured = true,
                    DisplayOrder = 3,
                    Slug = "sapa",
                    MetaTitle = "Sapa Trekking Tours - Rice Terraces & Mountain Views",
                    MetaDescription = "Trek through spectacular rice terraces and meet local ethnic minorities in Sapa.",
                    CreatedAt = new DateTime(2023, 1, 15),
                    IsDeleted = false
                },
                new Destination
                {
                    Id = 4,
                    Name = "Phu Quoc Island",
                    Country = "Vietnam",
                    Description = "Tropical paradise with pristine beaches, crystal-clear waters, and vibrant coral reefs.",
                    ImageUrl = "https://images.unsplash.com/photo-1583037189850-1921ae7c6c22",
                    AverageRating = 4.5m,
                    TotalReviews = 987,
                    StartingPrice = 2000000m,
                    TourCount = 18,
                    IsFeatured = true,
                    DisplayOrder = 4,
                    Slug = "phu-quoc-island",
                    MetaTitle = "Phu Quoc Island Tours - Beach Paradise",
                    MetaDescription = "Relax on stunning beaches and explore the natural beauty of Phu Quoc Island.",
                    CreatedAt = new DateTime(2023, 1, 15),
                    IsDeleted = false
                },
                new Destination
                {
                    Id = 5,
                    Name = "Da Nang",
                    Country = "Vietnam",
                    Description = "Modern coastal city with beautiful beaches, marble mountains, and the iconic Golden Bridge.",
                    ImageUrl = "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b",
                    AverageRating = 4.6m,
                    TotalReviews = 1567,
                    StartingPrice = 900000m,
                    TourCount = 14,
                    IsFeatured = false,
                    DisplayOrder = 5,
                    Slug = "da-nang",
                    MetaTitle = "Da Nang City Tours - Modern Vietnam",
                    MetaDescription = "Experience the perfect blend of beach, culture, and modern attractions in Da Nang.",
                    CreatedAt = new DateTime(2023, 1, 15),
                    IsDeleted = false
                }
            };

            modelBuilder.Entity<Destination>().HasData(destinations);

            // ===================================
            // 4. TAGS
            // ===================================
            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Adventure", Slug = "adventure", Icon = "🏔️", Color = "#FF6B6B", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 2, Name = "Cultural", Slug = "cultural", Icon = "🏛️", Color = "#4ECDC4", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 3, Name = "Beach", Slug = "beach", Icon = "🏖️", Color = "#45B7D1", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 4, Name = "Nature", Slug = "nature", Icon = "🌿", Color = "#96CEB4", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 5, Name = "Food & Cuisine", Slug = "food-cuisine", Icon = "🍜", Color = "#FFEAA7", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 6, Name = "Photography", Slug = "photography", Icon = "📷", Color = "#DFE6E9", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 7, Name = "Cruise", Slug = "cruise", Icon = "🚢", Color = "#74B9FF", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 8, Name = "Trekking", Slug = "trekking", Icon = "🥾", Color = "#A29BFE", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 9, Name = "Family Friendly", Slug = "family-friendly", Icon = "👨‍👩‍👧‍👦", Color = "#FD79A8", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false },
                new Tag { Id = 10, Name = "Luxury", Slug = "luxury", Icon = "💎", Color = "#FDCB6E", CreatedAt = new DateTime(2023, 1, 1), IsDeleted = false }
            };

            modelBuilder.Entity<Tag>().HasData(tags);

            // ===================================
            // 5. TOURS
            // ===================================
            var tours = new List<Tour>
            {
                new Tour
                {
                    Id = 1,
                    Name = "Ha Long Bay Luxury Cruise - 2 Days 1 Night",
                    Description = "Experience the magic of Ha Long Bay aboard a luxury cruise ship. Enjoy kayaking, cave exploration, and stunning sunset views over the limestone karsts.",
                    DestinationId = 1,
                    Location = "Ha Long Bay, Quang Ninh",
                    Price = 3500000m,
                    Duration = "2 days 1 night",
                    DurationDays = 2,
                    MaxGuests = 20,
                    Type = TourType.Luxury,
                    Category = TourCategory.Cruise,
                    Difficulty = null,
                    Status = TourStatus.Active,
                    IsFeatured = true,
                    AverageRating = 4.8m,
                    TotalReviews = 234,
                    TotalBookings = 567,
                    TotalRevenue = 198450000m,
                    MinAge = 5,
                    PhysicalRequirements = "Moderate walking ability required",
                    Slug = "ha-long-bay-luxury-cruise-2d1n",
                    MetaTitle = "Ha Long Bay Luxury Cruise 2D1N - Premium Experience",
                    MetaDescription = "Luxury cruise experience in Ha Long Bay with cave exploration and kayaking.",
                    CreatedAt = new DateTime(2023, 2, 1),
                    IsDeleted = false
                },
                new Tour
                {
                    Id = 2,
                    Name = "Hoi An Walking Food Tour",
                    Description = "Discover the culinary delights of Hoi An's ancient town. Sample authentic street food, visit local markets, and learn about Vietnamese cuisine from expert guides.",
                    DestinationId = 2,
                    Location = "Hoi An Ancient Town",
                    Price = 850000m,
                    Duration = "4 hours",
                    DurationDays = 1,
                    MaxGuests = 12,
                    Type = TourType.Culinary,
                    Category = TourCategory.Culinary,
                    Difficulty = null,
                    Status = TourStatus.Active,
                    IsFeatured = true,
                    AverageRating = 4.9m,
                    TotalReviews = 389,
                    TotalBookings = 892,
                    TotalRevenue = 75820000m,
                    MinAge = 8,
                    Slug = "hoi-an-walking-food-tour",
                    MetaTitle = "Hoi An Food Tour - Authentic Vietnamese Cuisine",
                    MetaDescription = "Taste the best of Hoi An's street food and local delicacies on this guided walking tour.",
                    CreatedAt = new DateTime(2023, 2, 5),
                    IsDeleted = false
                },
                new Tour
                {
                    Id = 3,
                    Name = "Sapa Trekking Adventure - 3 Days 2 Nights",
                    Description = "Trek through stunning rice terraces and visit authentic ethnic minority villages. Experience homestays and learn about traditional mountain life.",
                    DestinationId = 3,
                    Location = "Sapa, Lao Cai",
                    Price = 4200000m,
                    Duration = "3 days 2 nights",
                    DurationDays = 3,
                    MaxGuests = 15,
                    Type = TourType.Adventure,
                    Category = TourCategory.Trekking,
                    Difficulty = TourDifficulty.Moderate,
                    Status = TourStatus.Active,
                    IsFeatured = true,
                    AverageRating = 4.7m,
                    TotalReviews = 178,
                    TotalBookings = 345,
                    TotalRevenue = 144900000m,
                    MinAge = 12,
                    PhysicalRequirements = "Good fitness level required. 5-7 hours of trekking daily.",
                    SpecialRequirements = "Warm clothing, sturdy hiking boots, rain gear",
                    Slug = "sapa-trekking-adventure-3d2n",
                    MetaTitle = "Sapa Trekking Tour 3D2N - Rice Terraces & Villages",
                    MetaDescription = "Multi-day trekking adventure through Sapa's stunning landscapes and ethnic villages.",
                    CreatedAt = new DateTime(2023, 2, 10),
                    IsDeleted = false
                },
                new Tour
                {
                    Id = 4,
                    Name = "Phu Quoc Island Snorkeling & Beach Day",
                    Description = "Explore the underwater paradise of Phu Quoc. Snorkel in crystal-clear waters, relax on pristine beaches, and enjoy a fresh seafood lunch.",
                    DestinationId = 4,
                    Location = "Phu Quoc Island",
                    Price = 1800000m,
                    Duration = "Full day (8 hours)",
                    DurationDays = 1,
                    MaxGuests = 25,
                    Type = TourType.Beach,
                    Category = TourCategory.Beach,
                    Difficulty = TourDifficulty.Easy,
                    Status = TourStatus.Active,
                    IsFeatured = true,
                    AverageRating = 4.6m,
                    TotalReviews = 456,
                    TotalBookings = 789,
                    TotalRevenue = 142020000m,
                    MinAge = 6,
                    PhysicalRequirements = "Basic swimming ability recommended",
                    Slug = "phu-quoc-snorkeling-beach-day",
                    MetaTitle = "Phu Quoc Snorkeling Tour - Island Paradise",
                    MetaDescription = "Full-day beach and snorkeling tour exploring Phu Quoc's best underwater spots.",
                    CreatedAt = new DateTime(2023, 2, 15),
                    IsDeleted = false
                },
                new Tour
                {
                    Id = 5,
                    Name = "Da Nang Highlights & Golden Bridge",
                    Description = "Visit Da Nang's most iconic attractions including the Golden Bridge, Marble Mountains, and Lady Buddha statue. Perfect city tour for first-time visitors.",
                    DestinationId = 5,
                    Location = "Da Nang City",
                    Price = 1200000m,
                    Duration = "Full day (9 hours)",
                    DurationDays = 1,
                    MaxGuests = 30,
                    Type = TourType.City,
                    Category = TourCategory.City,
                    Difficulty = null,
                    Status = TourStatus.Active,
                    IsFeatured = false,
                    AverageRating = 4.5m,
                    TotalReviews = 267,
                    TotalBookings = 534,
                    TotalRevenue = 64080000m,
                    MinAge = 3,
                    Slug = "da-nang-highlights-golden-bridge",
                    MetaTitle = "Da Nang City Tour - Golden Bridge & Marble Mountains",
                    MetaDescription = "Comprehensive city tour covering Da Nang's must-see attractions and landmarks.",
                    CreatedAt = new DateTime(2023, 2, 20),
                    IsDeleted = false
                },
                new Tour
                {
                    Id = 6,
                    Name = "Hoi An Lantern Making Workshop",
                    Description = "Learn the traditional art of lantern making from local artisans. Create your own beautiful lantern to take home as a unique souvenir.",
                    DestinationId = 2,
                    Location = "Hoi An Ancient Town",
                    Price = 650000m,
                    Duration = "3 hours",
                    DurationDays = 1,
                    MaxGuests = 10,
                    Type = TourType.Cultural,
                    Category = TourCategory.Cultural,
                    Difficulty = null,
                    Status = TourStatus.Active,
                    IsFeatured = false,
                    AverageRating = 4.8m,
                    TotalReviews = 145,
                    TotalBookings = 298,
                    TotalRevenue = 19370000m,
                    MinAge = 7,
                    Slug = "hoi-an-lantern-making-workshop",
                    MetaTitle = "Hoi An Lantern Workshop - Traditional Craft Experience",
                    MetaDescription = "Hands-on workshop to create your own traditional Vietnamese lantern in Hoi An.",
                    CreatedAt = new DateTime(2023, 3, 1),
                    IsDeleted = false
                }
            };

            modelBuilder.Entity<Tour>().HasData(tours);

         
        }
    }
}