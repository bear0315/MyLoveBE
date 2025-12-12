using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Repository
{
    public class TourRepository : GenericRepository<Tour>, ITourRepository
    {
        public TourRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<Tour?> GetTourDetailAsync(int id)
        {
            return await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Itineraries.OrderBy(i => i.DayNumber))
                .Include(t => t.Includes.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Excludes.OrderBy(e => e.DisplayOrder))
                .Include(t => t.TourGuides).ThenInclude(tg => tg.Guide)
                .Include(t => t.TourTags).ThenInclude(tt => tt.Tag)
                .Include(t => t.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tour?> GetTourBySlugAsync(string slug)
        {
            return await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Itineraries.OrderBy(i => i.DayNumber))
                .Include(t => t.Includes.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Excludes.OrderBy(e => e.DisplayOrder))
                .Include(t => t.TourGuides).ThenInclude(tg => tg.Guide)
                .Include(t => t.TourTags).ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Slug == slug && t.Status == TourStatus.Active);
        }

        public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
        {
            var query = _context.Tours.Where(t => t.Slug == slug);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Tour>> GetFeaturedToursAsync(int take = 10)
        {
            return await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.Where(i => i.IsPrimary))
                .Where(t => t.IsFeatured && t.Status == TourStatus.Active)
                .OrderByDescending(t => t.AverageRating)
                .ThenByDescending(t => t.TotalBookings)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetPopularToursAsync(int take = 10)
        {
            return await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.Where(i => i.IsPrimary))
                .Where(t => t.Status == TourStatus.Active)
                .OrderByDescending(t => t.TotalBookings)
                .ThenByDescending(t => t.AverageRating)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetRelatedToursAsync(int tourId, int take = 5)
        {
            var tour = await _context.Tours
                .Include(t => t.TourTags)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null) return new List<Tour>();

            var tagIds = tour.TourTags.Select(tt => tt.TagId).ToList();

            return await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.Where(i => i.IsPrimary))
                .Where(t => t.Id != tourId &&
                           t.Status == TourStatus.Active &&
                           (t.DestinationId == tour.DestinationId ||
                            t.Category == tour.Category ||
                            t.TourTags.Any(tt => tagIds.Contains(tt.TagId))))
                .OrderByDescending(t => t.AverageRating)
                .Take(take)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Tour> Tours, int TotalCount)> SearchToursAsync(
     string? keyword = null,
     int? destinationId = null,
     TourCategory? category = null,
     TourType? type = null,
     TourDifficulty? difficulty = null,
     TourStatus? status = null, 
     decimal? minPrice = null,
     decimal? maxPrice = null,
     int? minDays = null,
     int? maxDays = null,
     int? minRating = null,
     List<int>? tagIds = null,
     int pageNumber = 1,
     int pageSize = 10,
     string sortBy = "created",
     bool sortDesc = true,
     bool includeAllStatuses = false) 
        {
            var query = _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Images.Where(i => i.IsPrimary))
                .AsQueryable();

            if (!includeAllStatuses)
            {
                query = query.Where(t => t.Status == TourStatus.Active);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Keyword search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t =>
                    t.Name.Contains(keyword) ||
                    t.Description.Contains(keyword) ||
                    t.Location.Contains(keyword));
            }

            // Filters (giữ nguyên)
            if (destinationId.HasValue)
                query = query.Where(t => t.DestinationId == destinationId.Value);
            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);
            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);
            if (difficulty.HasValue)
                query = query.Where(t => t.Difficulty == difficulty.Value);
            if (minPrice.HasValue)
                query = query.Where(t => t.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(t => t.Price <= maxPrice.Value);
            if (minDays.HasValue)
                query = query.Where(t => t.DurationDays >= minDays.Value);
            if (maxDays.HasValue)
                query = query.Where(t => t.DurationDays <= maxDays.Value);
            if (minRating.HasValue)
                query = query.Where(t => t.AverageRating >= minRating.Value);

            // Tag filter
            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(t => t.TourTags.Any(tt => tagIds.Contains(tt.TagId)));
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Sorting
            query = sortBy.ToLower() switch
            {
                "price" => sortDesc ? query.OrderByDescending(t => t.Price) : query.OrderBy(t => t.Price),
                "rating" => sortDesc ? query.OrderByDescending(t => t.AverageRating) : query.OrderBy(t => t.AverageRating),
                "popularity" => sortDesc ? query.OrderByDescending(t => t.TotalBookings) : query.OrderBy(t => t.TotalBookings),
                "duration" => sortDesc ? query.OrderByDescending(t => t.DurationDays) : query.OrderBy(t => t.DurationDays),
                "name" => sortDesc ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                _ => sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
            };

            // Pagination
            var tours = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tours, totalCount);
        }

        public async Task<int> GetTotalActiveToursAsync()
        {
            return await _context.Tours.CountAsync(t => t.Status == TourStatus.Active);
        }

        public async Task<int> GetTotalBookingsForTourAsync(int tourId)
        {
            return await _context.Bookings
                .CountAsync(b => b.TourId == tourId &&
                               b.Status != BookingStatus.Cancelled);
        }

        public async Task<decimal> GetTotalRevenueForTourAsync(int tourId)
        {
            return await _context.Bookings
                .Where(b => b.TourId == tourId &&
                           b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => b.TotalAmount);
        }

        public async Task UpdateTourStatisticsAsync(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return;

            tour.TotalBookings = await GetTotalBookingsForTourAsync(tourId);
            tour.TotalRevenue = await GetTotalRevenueForTourAsync(tourId);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTourRatingAsync(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return;

            var reviews = await _context.Reviews
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            tour.TotalReviews = reviews.Count;
            tour.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTourAvailableAsync(int tourId, DateTime tourDate, int numberOfGuests)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null || tour.Status != TourStatus.Active) return false;

            // Check if tour date is in the future
            if (tourDate.Date < DateTime.Now.Date) return false;

            // Check max guests
            var bookedGuests = await _context.Bookings
                .Where(b => b.TourId == tourId &&
                           b.TourDate.Date == tourDate.Date &&
                           b.Status != BookingStatus.Cancelled)
                .SumAsync(b => b.NumberOfGuests);

            return (bookedGuests + numberOfGuests) <= tour.MaxGuests;
        }

        public async Task<IEnumerable<DateTime>> GetAvailableDatesAsync(int tourId, DateTime fromDate, DateTime toDate)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return new List<DateTime>();

            var bookedDates = await _context.Bookings
                .Where(b => b.TourId == tourId &&
                           b.TourDate >= fromDate &&
                           b.TourDate <= toDate &&
                           b.Status != BookingStatus.Cancelled)
                .GroupBy(b => b.TourDate.Date)
                .Select(g => new { Date = g.Key, TotalGuests = g.Sum(b => b.NumberOfGuests) })
                .Where(x => x.TotalGuests >= tour.MaxGuests)
                .Select(x => x.Date)
                .ToListAsync();

            var availableDates = new List<DateTime>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                if (!bookedDates.Contains(date) && date >= DateTime.Now.Date)
                {
                    availableDates.Add(date);
                }
            }

            return availableDates;
        }

        public async Task<bool> BulkUpdateStatusAsync(List<int> tourIds, TourStatus status)
        {
            var tours = await _context.Tours.Where(t => tourIds.Contains(t.Id)).ToListAsync();
            foreach (var tour in tours)
            {
                tour.Status = status;
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BulkUpdateFeaturedAsync(List<int> tourIds, bool isFeatured)
        {
            var tours = await _context.Tours.Where(t => tourIds.Contains(t.Id)).ToListAsync();
            foreach (var tour in tours)
            {
                tour.IsFeatured = isFeatured;
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BulkDeleteAsync(List<int> tourIds)
        {
            var tours = await _context.Tours.Where(t => tourIds.Contains(t.Id)).ToListAsync();
            _context.Tours.RemoveRange(tours);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
