using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDestinationRepository : IGenericRepository<Destination>
    {
        Task<IEnumerable<Destination>> GetActiveDestinationsAsync();
        Task<Destination?> GetBySlugAsync(string slug);
        Task<IEnumerable<Destination>> GetPopularDestinationsAsync(int take = 10);
        Task<IEnumerable<Destination>> GetFeaturedDestinationsAsync(int take = 10);
        Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);
        Task<IEnumerable<Destination>> GetDestinationsWithToursAsync();

        Task<(IEnumerable<Destination> Destinations, int TotalCount)> SearchDestinationsAsync(
            string? keyword = null,
            string? country = null,
            bool? isFeatured = null,
            decimal? minRating = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "name",
            bool sortDesc = false);

        Task<int> GetTotalToursCountAsync(int destinationId);
        Task UpdateDestinationStatisticsAsync(int destinationId);
        Task<IEnumerable<string>> GetAllCountriesAsync();
        Task UpdateDisplayOrderAsync(Dictionary<int, int> displayOrders);
        Task<bool> ToggleFeaturedAsync(int destinationId);
    }
}
