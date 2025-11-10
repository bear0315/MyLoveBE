using Application.Interfaces;
using Application.Request.Review;
using Application.Response.Common;
using Application.Response.Review;
using Domain.Entities.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewImageRepository _reviewImageRepository;
        private readonly ITourRepository _tourRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            ITourRepository tourRepository,
            IReviewImageRepository reviewImageRepository)
        {
            _reviewRepository = reviewRepository;
            _tourRepository = tourRepository;
            _reviewImageRepository = reviewImageRepository;
        }

        public async Task<ReviewResponse?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review == null ? null : MapToResponse(review);
        }

        public async Task<PagedResult<ReviewResponse>> GetTourReviewsAsync(
            int tourId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var reviews = await _reviewRepository.GetByTourIdAsync(tourId, pageNumber, pageSize);
            var totalCount = await _reviewRepository.CountReviewsAsync(tourId, ReviewStatus.Approved);

            return new PagedResult<ReviewResponse>
            {
                Items = reviews.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<ReviewResponse>> GetUserReviewsAsync(int userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return reviews.Select(MapToResponse).ToList();
        }

        public async Task<ReviewResponse?> GetReviewByBookingAsync(int bookingId)
        {
            var review = await _reviewRepository.GetByBookingIdAsync(bookingId);
            return review == null ? null : MapToResponse(review);
        }

        public async Task<ReviewStatisticsResponse> GetTourReviewStatisticsAsync(int tourId)
        {
            var (avgRating, totalReviews) = await _reviewRepository.GetTourRatingStatsAsync(tourId);
            var distribution = await _reviewRepository.GetRatingDistributionAsync(tourId);

            var stats = new ReviewStatisticsResponse
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(avgRating, 2),
                RatingDistribution = distribution,
                FiveStarCount = distribution.GetValueOrDefault(5, 0),
                FourStarCount = distribution.GetValueOrDefault(4, 0),
                ThreeStarCount = distribution.GetValueOrDefault(3, 0),
                TwoStarCount = distribution.GetValueOrDefault(2, 0),
                OneStarCount = distribution.GetValueOrDefault(1, 0)
            };

            // Calculate percentages
            if (totalReviews > 0)
            {
                stats.FiveStarPercentage = Math.Round((double)stats.FiveStarCount / totalReviews * 100, 1);
                stats.FourStarPercentage = Math.Round((double)stats.FourStarCount / totalReviews * 100, 1);
                stats.ThreeStarPercentage = Math.Round((double)stats.ThreeStarCount / totalReviews * 100, 1);
                stats.TwoStarPercentage = Math.Round((double)stats.TwoStarCount / totalReviews * 100, 1);
                stats.OneStarPercentage = Math.Round((double)stats.OneStarCount / totalReviews * 100, 1);
            }

            return stats;
        }

        public async Task<ReviewSummaryResponse> GetTourReviewSummaryAsync(int tourId)
        {
            var tour = await _tourRepository.GetByIdAsync(tourId);
            if (tour == null)
                throw new KeyNotFoundException($"Tour with ID {tourId} not found");

            var statistics = await GetTourReviewStatisticsAsync(tourId);
            var recentReviews = await _reviewRepository.GetByTourIdAsync(tourId, 1, 5);

            var recommendationCount = statistics.FiveStarCount + statistics.FourStarCount;
            var recommendationPercentage = statistics.TotalReviews > 0
                ? (int)Math.Round((double)recommendationCount / statistics.TotalReviews * 100)
                : 0;

            return new ReviewSummaryResponse
            {
                TourId = tourId,
                TourName = tour.Name,
                AverageRating = statistics.AverageRating,
                TotalReviews = statistics.TotalReviews,
                RecommendationPercentage = recommendationPercentage,
                RecentReviews = recentReviews.Select(MapToResponse).ToList(),
                Statistics = statistics
            };
        }

        public async Task<bool> HasUserReviewedTourAsync(int userId, int tourId)
        {
            return await _reviewRepository.HasUserReviewedTourAsync(userId, tourId);
        }

        public async Task<PagedResult<ReviewResponse>> SearchReviewsAsync(ReviewSearchRequest request)
        {
            var reviews = await _reviewRepository.SearchReviewsAsync(
                tourId: request.TourId,
                userId: request.UserId,
                status: request.Status,
                minRating: request.MinRating,
                maxRating: request.MaxRating,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                sortDesc: request.SortDesc);

            var totalCount = await _reviewRepository.CountReviewsAsync(request.TourId, request.Status);

            return new PagedResult<ReviewResponse>
            {
                Items = reviews.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ReviewResponse> CreateReviewAsync(int userId, CreateReviewRequest request)
        {
            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            // Check if tour exists
            var tour = await _tourRepository.GetByIdAsync(request.TourId);
            if (tour == null)
                throw new KeyNotFoundException($"Tour with ID {request.TourId} not found");

            // Check if user already reviewed this tour
            var hasReviewed = await _reviewRepository.HasUserReviewedTourAsync(userId, request.TourId);
            if (hasReviewed)
                throw new InvalidOperationException("You have already reviewed this tour");

            var review = new Domain.Entities.Review
            {
                UserId = userId,
                TourId = request.TourId,
                BookingId = request.BookingId,
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _reviewRepository.AddAsync(review);

            // Add images if provided
            if (request.Images != null && request.Images.Any())
            {
                var reviewImages = request.Images.Select(imageUrl => new Domain.Entities.ReviewImage
                {
                    ReviewId = created.Id,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _reviewImageRepository.BulkAddAsync(reviewImages);
            }

            // Update tour rating
            await _tourRepository.UpdateTourRatingAsync(request.TourId);

            // Reload with images
            var reviewWithImages = await _reviewRepository.GetByIdAsync(created.Id);
            return MapToResponse(reviewWithImages!);
        }

        public async Task<ReviewResponse> UpdateReviewAsync(
            int userId,
            int reviewId,
            UpdateReviewRequest request)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found");

            // Check ownership
            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own reviews");

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            review.Rating = request.Rating;
            review.Title = request.Title;
            review.Comment = request.Comment;
            review.Status = ReviewStatus.Pending; // Reset to pending after edit
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review);

            // Update images if provided
            if (request.Images != null)
            {
                // Delete old images
                await _reviewImageRepository.BulkDeleteByReviewIdAsync(reviewId);

                // Add new images
                if (request.Images.Any())
                {
                    var reviewImages = request.Images.Select(imageUrl => new Domain.Entities.ReviewImage
                    {
                        ReviewId = reviewId,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    await _reviewImageRepository.BulkAddAsync(reviewImages);
                }
            }

            // Update tour rating
            await _tourRepository.UpdateTourRatingAsync(review.TourId);

            // Reload with images
            var updatedReview = await _reviewRepository.GetByIdAsync(reviewId);
            return MapToResponse(updatedReview!);
        }

        public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            // Check ownership
            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews");

            var tourId = review.TourId;

            // Delete images first
            await _reviewImageRepository.BulkDeleteByReviewIdAsync(reviewId);

            // Delete review
            await _reviewRepository.DeleteAsync(review);

            // Update tour rating
            await _tourRepository.UpdateTourRatingAsync(tourId);

            return true;
        }

        public async Task<bool> ApproveReviewAsync(int reviewId, int approvedBy)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            await _reviewRepository.ApproveReviewAsync(reviewId, approvedBy);

            // Update tour rating
            await _tourRepository.UpdateTourRatingAsync(review.TourId);

            return true;
        }

        public async Task<bool> RejectReviewAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            await _reviewRepository.RejectReviewAsync(reviewId);
            return true;
        }

        public async Task<bool> MarkReviewHelpfulAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.HelpfulCount++;
            review.UpdatedAt = DateTime.UtcNow;
            await _reviewRepository.UpdateAsync(review);
            return true;
        }

        public async Task<PagedResult<ReviewResponse>> GetPendingReviewsAsync(int pageNumber = 1, int pageSize = 20)
        {
            var reviews = await _reviewRepository.SearchReviewsAsync(
                status: ReviewStatus.Pending,
                pageNumber: pageNumber,
                pageSize: pageSize,
                sortBy: "created",
                sortDesc: false); // Oldest first

            var totalCount = await _reviewRepository.CountReviewsAsync(status: ReviewStatus.Pending);

            return new PagedResult<ReviewResponse>
            {
                Items = reviews.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetPendingReviewCountAsync()
        {
            return await _reviewRepository.CountReviewsAsync(status: ReviewStatus.Pending);
        }

        private ReviewResponse MapToResponse(Domain.Entities.Review review)
        {
            return new ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User?.FullName ?? "",
                UserAvatar = review.User?.Avatar,
                TourId = review.TourId,
                TourName = review.Tour?.Name ?? "",
                BookingId = review.BookingId,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                Status = review.Status,
                HelpfulCount = review.HelpfulCount,
                CreatedAt = review.CreatedAt,
                ApprovedAt = review.ApprovedAt,
                Images = review.Images?.Select(i => new ReviewImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }
    }
}
