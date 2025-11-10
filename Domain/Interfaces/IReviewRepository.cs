using Domain.Entities;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetByTourIdAsync(int tourId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
        Task<Review?> GetByBookingIdAsync(int bookingId);
        Task<bool> HasUserReviewedTourAsync(int userId, int tourId);
        Task<(double AverageRating, int TotalReviews)> GetTourRatingStatsAsync(int tourId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int tourId);
        Task ApproveReviewAsync(int reviewId, int approvedBy);
        Task RejectReviewAsync(int reviewId);
        Task<IEnumerable<Review>> SearchReviewsAsync(
            int? tourId = null,
            int? userId = null,
            ReviewStatus? status = null,
            int? minRating = null,
            int? maxRating = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created",
            bool sortDesc = true);
        Task<int> CountReviewsAsync(int? tourId = null, ReviewStatus? status = null);
    }
}
