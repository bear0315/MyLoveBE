using Application.Interfaces;
using Application.Request.Destination;
using Application.Response.Common;
using Application.Response.Destination;
using Application.Response.Tour;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly IDestinationRepository _destinationRepository;

        public DestinationService(IDestinationRepository destinationRepository)
        {
            _destinationRepository = destinationRepository;
        }

        public async Task<DestinationDetailResponse?> GetDestinationByIdAsync(int id)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            return destination == null ? null : MapToDetailResponse(destination);
        }

        public async Task<DestinationDetailResponse?> GetDestinationBySlugAsync(string slug)
        {
            var destination = await _destinationRepository.GetBySlugAsync(slug);
            return destination == null ? null : MapToDetailResponse(destination);
        }

        public async Task<PagedResult<DestinationListResponse>> GetAllDestinationsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (destinations, totalCount) = await _destinationRepository.SearchDestinationsAsync(
                pageNumber: pageNumber,
                pageSize: pageSize);

            return new PagedResult<DestinationListResponse>
            {
                Items = destinations.Select(MapToListResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<DestinationListResponse>> GetActiveDestinationsAsync()
        {
            var destinations = await _destinationRepository.GetActiveDestinationsAsync();
            return destinations.Select(MapToListResponse).ToList();
        }

        public async Task<List<DestinationListResponse>> GetPopularDestinationsAsync(int take = 10)
        {
            var destinations = await _destinationRepository.GetPopularDestinationsAsync(take);
            return destinations.Select(MapToListResponse).ToList();
        }

        public async Task<List<DestinationListResponse>> GetFeaturedDestinationsAsync(int take = 10)
        {
            var destinations = await _destinationRepository.GetFeaturedDestinationsAsync(take);
            return destinations.Select(MapToListResponse).ToList();
        }

        public async Task<PagedResult<DestinationListResponse>> SearchDestinationsAsync(DestinationSearchRequest request)
        {
            var (destinations, totalCount) = await _destinationRepository.SearchDestinationsAsync(
                keyword: request.Keyword,
                country: request.Country,
                isFeatured: request.IsFeatured,
                minRating: request.MinRating,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                sortDesc: request.SortDesc);

            return new PagedResult<DestinationListResponse>
            {
                Items = destinations.Select(MapToListResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<DestinationDetailResponse> CreateDestinationAsync(CreateDestinationRequest request)
        {
            var slug = GenerateSlug(request.Name);
            var slugExists = await _destinationRepository.IsSlugExistsAsync(slug);
            if (slugExists)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            }

            var destination = new Destination
            {
                Name = request.Name,
                Country = request.Country,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                IsFeatured = request.IsFeatured,
                DisplayOrder = request.DisplayOrder,
                Slug = slug,
                MetaTitle = request.MetaTitle ?? request.Name,
                MetaDescription = request.MetaDescription
            };

            var created = await _destinationRepository.AddAsync(destination);
            return MapToDetailResponse(created);
        }

        public async Task<DestinationDetailResponse> UpdateDestinationAsync(int id, UpdateDestinationRequest request)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            if (destination == null)
                throw new KeyNotFoundException($"Destination with ID {id} not found");

            if (destination.Name != request.Name)
            {
                var newSlug = GenerateSlug(request.Name);
                var slugExists = await _destinationRepository.IsSlugExistsAsync(newSlug, id);
                if (slugExists)
                {
                    newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
                }
                destination.Slug = newSlug;
            }

            destination.Name = request.Name;
            destination.Country = request.Country;
            destination.Description = request.Description;
            destination.ImageUrl = request.ImageUrl;
            destination.IsFeatured = request.IsFeatured;
            destination.DisplayOrder = request.DisplayOrder;
            destination.MetaTitle = request.MetaTitle ?? request.Name;
            destination.MetaDescription = request.MetaDescription;

            await _destinationRepository.UpdateAsync(destination);
            return MapToDetailResponse(destination);
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            if (destination == null) return false;

            await _destinationRepository.DeleteAsync(destination);
            return true;
        }

        public async Task<bool> ToggleFeaturedAsync(int id)
        {
            return await _destinationRepository.ToggleFeaturedAsync(id);
        }

        public async Task UpdateDestinationStatisticsAsync(int id)
        {
            await _destinationRepository.UpdateDestinationStatisticsAsync(id);
        }

        public async Task<List<string>> GetAllCountriesAsync()
        {
            var countries = await _destinationRepository.GetAllCountriesAsync();
            return countries.ToList();
        }

        public async Task<bool> UpdateDisplayOrderAsync(Dictionary<int, int> displayOrders)
        {
            await _destinationRepository.UpdateDisplayOrderAsync(displayOrders);
            return true;
        }

        public string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("đ", "d");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            return slug.Trim('-');
        }

        private DestinationListResponse MapToListResponse(Destination d)
        {
            return new DestinationListResponse
            {
                Id = d.Id,
                Name = d.Name,
                Country = d.Country,
                Slug = d.Slug,
                ImageUrl = d.ImageUrl,
                AverageRating = d.AverageRating,
                TotalReviews = d.TotalReviews,
                StartingPrice = d.StartingPrice,
                TourCount = d.TourCount,
                IsFeatured = d.IsFeatured
            };
        }

        private DestinationDetailResponse MapToDetailResponse(Destination d)
        {
            return new DestinationDetailResponse
            {
                Id = d.Id,
                Name = d.Name,
                Country = d.Country,
                Slug = d.Slug,
                Description = d.Description,
                ImageUrl = d.ImageUrl,
                AverageRating = d.AverageRating,
                TotalReviews = d.TotalReviews,
                StartingPrice = d.StartingPrice,
                TourCount = d.TourCount,
                IsFeatured = d.IsFeatured,
                DisplayOrder = d.DisplayOrder,
                MetaTitle = d.MetaTitle,
                MetaDescription = d.MetaDescription,
                Tours = d.Tours?.Select(t => new TourListResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    Location = t.Location,
                    Price = t.Price,
                    Duration = t.Duration,
                    AverageRating = t.AverageRating,
                    TotalReviews = t.TotalReviews
                }).ToList()
            };
        }
    }
}
