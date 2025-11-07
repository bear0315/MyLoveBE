using Domain.Entities;
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
                .Where(r => r.TourId == tourId && r.Status == Domain.Entities.Enums.ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Tour)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Reviews
                .Include(r => r.User)
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
                .Where(r => r.TourId == tourId && r.Status == Domain.Entities.Enums.ReviewStatus.Approved)
                .ToListAsync();

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0 ? reviews.Average(r => r.Rating) : 0;

            return (averageRating, totalReviews);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int tourId)
        {
            return await _context.Reviews
                .Where(r => r.TourId == tourId && r.Status == Domain.Entities.Enums.ReviewStatus.Approved)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);
        }

        public async Task ApproveReviewAsync(int reviewId, int approvedBy)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.Status = Domain.Entities.Enums.ReviewStatus.Approved;
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
                review.Status = Domain.Entities.Enums.ReviewStatus.Rejected;
                await _context.SaveChangesAsync();
            }
        }
    }
}
