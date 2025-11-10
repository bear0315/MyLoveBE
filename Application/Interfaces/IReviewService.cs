using Application.Request.Review;
using Application.Response.Common;
using Application.Response.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        // Query
        Task<ReviewResponse?> GetReviewByIdAsync(int id);
        Task<PagedResult<ReviewResponse>> GetTourReviewsAsync(int tourId, int pageNumber = 1, int pageSize = 10);
        Task<List<ReviewResponse>> GetUserReviewsAsync(int userId);
        Task<ReviewResponse?> GetReviewByBookingAsync(int bookingId);
        Task<ReviewStatisticsResponse> GetTourReviewStatisticsAsync(int tourId);
        Task<ReviewSummaryResponse> GetTourReviewSummaryAsync(int tourId);
        Task<bool> HasUserReviewedTourAsync(int userId, int tourId);
        Task<PagedResult<ReviewResponse>> SearchReviewsAsync(ReviewSearchRequest request);

        // Command
        Task<ReviewResponse> CreateReviewAsync(int userId, CreateReviewRequest request);
        Task<ReviewResponse> UpdateReviewAsync(int userId, int reviewId, UpdateReviewRequest request);
        Task<bool> DeleteReviewAsync(int userId, int reviewId);
        Task<bool> ApproveReviewAsync(int reviewId, int approvedBy);
        Task<bool> RejectReviewAsync(int reviewId);
        Task<bool> MarkReviewHelpfulAsync(int reviewId);

        // Admin
        Task<PagedResult<ReviewResponse>> GetPendingReviewsAsync(int pageNumber = 1, int pageSize = 20);
        Task<int> GetPendingReviewCountAsync();
    }
}
