using Domain.Entities.Enums;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourRepository : IGenericRepository<Tour>
    {
        // Tour Management
        Task<Tour?> GetTourDetailAsync(int id);
        Task<Tour?> GetTourBySlugAsync(string slug);
        Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);
        Task<IEnumerable<Tour>> GetFeaturedToursAsync(int take = 10);
        Task<IEnumerable<Tour>> GetPopularToursAsync(int take = 10);
        Task<IEnumerable<Tour>> GetRelatedToursAsync(int tourId, int take = 5);

        // Filtering & Search
        Task<(IEnumerable<Tour> Tours, int TotalCount)> SearchToursAsync(
            string? keyword = null,
            int? destinationId = null,
            TourCategory? category = null,
            TourType? type = null,
            TourDifficulty? difficulty = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minDays = null,
            int? maxDays = null,
            int? minRating = null,
            List<int>? tagIds = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "created",
            bool sortDesc = true
        );

        // Statistics
        Task<int> GetTotalActiveToursAsync();
        Task<int> GetTotalBookingsForTourAsync(int tourId);
        Task<decimal> GetTotalRevenueForTourAsync(int tourId);
        Task UpdateTourStatisticsAsync(int tourId);
        Task UpdateTourRatingAsync(int tourId);

        // Tour Availability
        Task<bool> IsTourAvailableAsync(int tourId, DateTime tourDate, int numberOfGuests);
        Task<IEnumerable<DateTime>> GetAvailableDatesAsync(int tourId, DateTime fromDate, DateTime toDate);

        // Bulk Operations
        Task<bool> BulkUpdateStatusAsync(List<int> tourIds, TourStatus status);
        Task<bool> BulkUpdateFeaturedAsync(List<int> tourIds, bool isFeatured);
        Task<bool> BulkDeleteAsync(List<int> tourIds);
    }
}
