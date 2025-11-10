using Application.Request.Destination;
using Application.Response.Common;
using Application.Response.Destination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDestinationService
    {
        Task<DestinationDetailResponse?> GetDestinationByIdAsync(int id);
        Task<DestinationDetailResponse?> GetDestinationBySlugAsync(string slug);
        Task<PagedResult<DestinationListResponse>> GetAllDestinationsAsync(int pageNumber = 1, int pageSize = 10);
        Task<List<DestinationListResponse>> GetActiveDestinationsAsync();
        Task<List<DestinationListResponse>> GetPopularDestinationsAsync(int take = 10);
        Task<List<DestinationListResponse>> GetFeaturedDestinationsAsync(int take = 10);
        Task<PagedResult<DestinationListResponse>> SearchDestinationsAsync(DestinationSearchRequest request);
        Task<DestinationDetailResponse> CreateDestinationAsync(CreateDestinationRequest request);
        Task<DestinationDetailResponse> UpdateDestinationAsync(int id, UpdateDestinationRequest request);
        Task<bool> DeleteDestinationAsync(int id);
        Task<bool> ToggleFeaturedAsync(int id);
        Task UpdateDestinationStatisticsAsync(int id);
        Task<List<string>> GetAllCountriesAsync();
        Task<bool> UpdateDisplayOrderAsync(Dictionary<int, int> displayOrders);
        string GenerateSlug(string name);
    }
}
