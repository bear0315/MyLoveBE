using Application.Response.Guide;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GuideService : IGuideService
    {
        private readonly IGuideRepository _guideRepository;
        private readonly ITourGuideRepository _tourGuideRepository;

        public GuideService(
            IGuideRepository guideRepository,
            ITourGuideRepository tourGuideRepository)
        {
            _guideRepository = guideRepository;
            _tourGuideRepository = tourGuideRepository;
        }

        /// <summary>
        /// Lấy danh sách guide của một tour
        /// </summary>
        public async Task<List<GuideListResponse>> GetGuidesByTourIdAsync(int tourId)
        {
            var tourGuides = await _tourGuideRepository.GetByTourIdAsync(tourId);

            return tourGuides.Select(tg => new GuideListResponse
            {
                Id = tg.GuideId,
                FullName = tg.Guide.FullName,
                Email = tg.Guide.Email,
                PhoneNumber = tg.Guide.PhoneNumber,
                Avatar = tg.Guide.Avatar,
                Bio = tg.Guide.Bio,
                Languages = tg.Guide.Languages,
                ExperienceYears = tg.Guide.ExperienceYears,
                AverageRating = tg.Guide.AverageRating,
                TotalReviews = tg.Guide.TotalReviews,
                Status = tg.Guide.Status,
                IsDefault = tg.IsDefault
            }).ToList();
        }

        /// <summary>
        /// Lấy guide mặc định của tour
        /// </summary>
        public async Task<GuideListResponse?> GetDefaultGuideForTourAsync(int tourId)
        {
            var tourGuide = await _tourGuideRepository.GetDefaultGuideForTourAsync(tourId);

            if (tourGuide == null) return null;

            return new GuideListResponse
            {
                Id = tourGuide.GuideId,
                FullName = tourGuide.Guide.FullName,
                Email = tourGuide.Guide.Email,
                PhoneNumber = tourGuide.Guide.PhoneNumber,
                Avatar = tourGuide.Guide.Avatar,
                Bio = tourGuide.Guide.Bio,
                Languages = tourGuide.Guide.Languages,
                ExperienceYears = tourGuide.Guide.ExperienceYears,
                AverageRating = tourGuide.Guide.AverageRating,
                TotalReviews = tourGuide.Guide.TotalReviews,
                Status = tourGuide.Guide.Status,
                IsDefault = tourGuide.IsDefault
            };
        }

        /// <summary>
        /// Lấy danh sách guide available cho tour vào ngày cụ thể
        /// </summary>
        public async Task<List<GuideAvailabilityResponse>> GetAvailableGuidesAsync(
            int tourId,
            DateTime tourDate)
        {
            var tourGuides = await _tourGuideRepository.GetByTourIdAsync(tourId);
            var availableGuides = new List<GuideAvailabilityResponse>();

            foreach (var tg in tourGuides)
            {
                var isAvailable = await _tourGuideRepository
                    .IsGuideAvailableAsync(tg.GuideId, tourDate);

                availableGuides.Add(new GuideAvailabilityResponse
                {
                    Id = tg.GuideId,
                    FullName = tg.Guide.FullName,
                    Avatar = tg.Guide.Avatar,
                    Bio = tg.Guide.Bio,
                    Languages = tg.Guide.Languages,
                    ExperienceYears = tg.Guide.ExperienceYears,
                    AverageRating = tg.Guide.AverageRating,
                    TotalReviews = tg.Guide.TotalReviews,
                    IsDefault = tg.IsDefault,
                    IsAvailable = isAvailable
                });
            }

            // Sắp xếp: Available trước, default trước, rating cao trước
            return availableGuides
                .OrderByDescending(g => g.IsAvailable)
                .ThenByDescending(g => g.IsDefault)
                .ThenByDescending(g => g.AverageRating)
                .ToList();
        }

        /// <summary>
        /// Kiểm tra guide có available không
        /// </summary>
        public async Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate)
        {
            return await _tourGuideRepository.IsGuideAvailableAsync(guideId, tourDate);
        }

        /// <summary>
        /// Validate guide có thuộc tour không
        /// </summary>
        public async Task<bool> ValidateGuideForTourAsync(int tourId, int guideId)
        {
            var tourGuides = await _tourGuideRepository.GetByTourIdAsync(tourId);
            return tourGuides.Any(tg => tg.GuideId == guideId);
        }
    }
}
