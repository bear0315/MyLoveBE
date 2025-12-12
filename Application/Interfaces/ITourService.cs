using Application.Request.Tour;
using Application.Response.Common;
using Application.Response.Tour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITourService
    {
        // CRUD Operations
        Task<TourDetailResponse?> GetTourByIdAsync(int id);
        Task<TourDetailResponse?> GetTourBySlugAsync(string slug);
        Task<TourDetailResponse> CreateTourAsync(CreateTourRequest request);
        Task<TourDetailResponse> UpdateTourAsync(int id, UpdateTourRequest request);
        Task<bool> DeleteTourAsync(int id);
        Task<bool> ToggleFeaturedAsync(int id);
        Task<bool> UpdateStatusAsync(int id, Domain.Entities.Enums.TourStatus status);

        // Search & Filter
        Task<PagedResult<TourListResponse>> SearchToursAsync(TourSearchRequest request);
        Task<List<TourListResponse>> GetFeaturedToursAsync(int take = 10);
        Task<List<TourListResponse>> GetPopularToursAsync(int take = 10);
        Task<List<TourListResponse>> GetRelatedToursAsync(int tourId, int take = 5);
        Task<List<TourListResponse>> GetToursByDestinationAsync(int destinationId, int pageNumber = 1, int pageSize = 10);
        Task<List<TourListResponse>> GetToursByCategoryAsync(Domain.Entities.Enums.TourCategory category, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<TourListResponse>> GetAllToursAsync(
     int pageNumber = 1,
     int pageSize = 10,
     bool includeAllStatuses = false);

        // Statistics
        Task<TourStatisticsDto> GetTourStatisticsAsync();
        Task UpdateTourStatisticsAsync(int tourId);
        Task UpdateTourRatingAsync(int tourId);

        // Availability
        Task<bool> CheckTourAvailabilityAsync(int tourId, DateTime tourDate, int numberOfGuests);
        Task<List<DateTime>> GetAvailableDatesAsync(int tourId, DateTime fromDate, DateTime toDate);

        // Bulk Operations
        Task<bool> BulkUpdateStatusAsync(BulkUpdateStatusRequest request);
        Task<bool> BulkUpdateFeaturedAsync(List<int> tourIds, bool isFeatured);
        Task<bool> BulkDeleteToursAsync(List<int> tourIds);

        // Helper
        Task<bool> IsTourExistsAsync(int id);
        Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);
        string GenerateSlug(string name);
        /// <summary>
        /// Lấy danh sách guides available cho tour vào ngày cụ thể
        /// </summary>
        Task<List<AvailableGuideDto>> GetAvailableGuidesForTourAsync(int tourId, DateTime tourDate);

        /// <summary>
        /// Lấy thông tin chi tiết guide
        /// </summary>
        Task<GuideDetailDto?> GetGuideDetailAsync(int guideId);

        /// <summary>
        /// Kiểm tra guide có available cho tour vào ngày cụ thể không
        /// </summary>
        Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate);

        /// <summary>
        /// Lấy default guide cho tour
        /// </summary>
        Task<int?> GetDefaultGuideIdForTourAsync(int tourId, DateTime tourDate);
    }
}
