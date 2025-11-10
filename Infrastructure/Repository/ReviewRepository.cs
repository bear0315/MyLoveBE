using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetByTourIdAsync(int tourId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Images)
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Tour)
                .Include(r => r.Images)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task<bool> HasUserReviewedTourAsync(int userId, int tourId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.TourId == tourId);
        }

        public async Task<(double AverageRating, int TotalReviews)> GetTourRatingStatsAsync(int tourId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0 ? reviews.Average(r => r.Rating) : 0;

            return (averageRating, totalReviews);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int tourId)
        {
            var distribution = await _context.Reviews
                .Where(r => r.TourId == tourId && r.Status == ReviewStatus.Approved)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            // Ensure all ratings 1-5 are present
            for (int i = 1; i <= 5; i++)
            {
                if (!distribution.ContainsKey(i))
                    distribution[i] = 0;
            }

            return distribution;
        }

        public async Task ApproveReviewAsync(int reviewId, int approvedBy)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.Status = ReviewStatus.Approved;
                review.ApprovedAt = DateTime.UtcNow;
                review.ApprovedBy = approvedBy;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.Status = ReviewStatus.Rejected;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Review>> SearchReviewsAsync(
            int? tourId = null,
            int? userId = null,
            ReviewStatus? status = null,
            int? minRating = null,
            int? maxRating = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created",
            bool sortDesc = true)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .Include(r => r.Images)
                .AsQueryable();

            // Filters
            if (tourId.HasValue)
                query = query.Where(r => r.TourId == tourId.Value);

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "rating" => sortDesc ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
                "helpful" => sortDesc ? query.OrderByDescending(r => r.HelpfulCount) : query.OrderBy(r => r.HelpfulCount),
                _ => sortDesc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountReviewsAsync(int? tourId = null, ReviewStatus? status = null)
        {
            var query = _context.Reviews.AsQueryable();

            if (tourId.HasValue)
                query = query.Where(r => r.TourId == tourId.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query.CountAsync();
        }
    }
   
}
