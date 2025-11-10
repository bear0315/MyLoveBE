using Application.Interfaces;
using Application.Response.Common;
using Application.Response.Favorite;
using Application.Response.Tour;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly ITourRepository _tourRepository;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            ITourRepository tourRepository)
        {
            _favoriteRepository = favoriteRepository;
            _tourRepository = tourRepository;
        }

        public async Task<List<FavoriteResponse>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
            return favorites.Select(MapToResponse).ToList();
        }

        public async Task<PagedResult<TourListResponse>> GetUserFavoriteToursAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var tours = await _favoriteRepository.GetFavoriteToursByUserIdAsync(userId, pageNumber, pageSize);

            var allFavorites = await _favoriteRepository.GetByUserIdAsync(userId);
            var totalCount = allFavorites.Count();

            return new PagedResult<TourListResponse>
            {
                Items = tours.Select(t => new TourListResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    Location = t.Location,
                    Price = t.Price,
                    Duration = t.Duration,
                    DurationDays = t.DurationDays,
                    Type = t.Type,
                    Category = t.Category,
                    AverageRating = t.AverageRating,
                    TotalReviews = t.TotalReviews,
                    PrimaryImageUrl = t.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                    DestinationName = t.Destination?.Name ?? ""
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> IsFavoriteAsync(int userId, int tourId)
        {
            return await _favoriteRepository.IsFavoriteAsync(userId, tourId);
        }

        public async Task<FavoriteResponse> AddFavoriteAsync(int userId, int tourId)
        {
            // Check if tour exists
            var tour = await _tourRepository.GetByIdAsync(tourId);
            if (tour == null)
                throw new KeyNotFoundException($"Tour with ID {tourId} not found");

            // Check if already favorited
            var exists = await _favoriteRepository.IsFavoriteAsync(userId, tourId);
            if (exists)
                throw new InvalidOperationException("Tour is already in favorites");

            var favorite = new Domain.Entities.Favorite
            {
                UserId = userId,
                TourId = tourId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _favoriteRepository.AddAsync(favorite);

            // Load tour data for response
            var favoriteWithTour = await _favoriteRepository.GetByIdAsync(created.Id);
            return MapToResponse(favoriteWithTour);
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int tourId)
        {
            var favorite = await _favoriteRepository.GetByUserAndTourAsync(userId, tourId);
            if (favorite == null) return false;

            await _favoriteRepository.DeleteAsync(favorite);
            return true;
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int tourId)
        {
            await _favoriteRepository.ToggleFavoriteAsync(userId, tourId);
            return true;
        }

        public async Task<int> GetFavoriteCountByTourAsync(int tourId)
        {
            return await _favoriteRepository.CountByTourIdAsync(tourId);
        }

        public async Task<Dictionary<int, bool>> CheckMultipleFavoritesAsync(int userId, List<int> tourIds)
        {
            var result = new Dictionary<int, bool>();

            foreach (var tourId in tourIds)
            {
                result[tourId] = await _favoriteRepository.IsFavoriteAsync(userId, tourId);
            }

            return result;
        }

        private FavoriteResponse MapToResponse(Domain.Entities.Favorite favorite)
        {
            return new FavoriteResponse
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                TourId = favorite.TourId,
                TourName = favorite.Tour?.Name ?? "",
                TourSlug = favorite.Tour?.Slug ?? "",
                TourImageUrl = favorite.Tour?.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                TourPrice = favorite.Tour?.Price ?? 0,
                TourDuration = favorite.Tour?.Duration ?? "",
                TourRating = favorite.Tour?.AverageRating ?? 0,
                CreatedAt = favorite.CreatedAt
            };
        }
    }
}
