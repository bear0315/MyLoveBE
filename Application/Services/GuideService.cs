using Application.Interfaces;
using Application.Response.Guide;
using Domain.Entities;
using Domain.Entities.Enums;
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

        public async Task<List<GuideListResponse>> GetAllGuidesAsync()
        {
            var guides = await _guideRepository.GetAllAsync();
            return guides.Select(MapToListResponse).ToList();
        }

        public async Task<GuideDetailResponse?> GetGuideByIdAsync(int id)
        {
            var guide = await _guideRepository.GetByIdAsync(id);
            if (guide == null) return null;

            var tourGuides = await _tourGuideRepository.GetByGuideIdAsync(id);

            return new GuideDetailResponse
            {
                GuideId = guide.Id,
                UserId = guide.UserId,  // ✅ Thêm UserId
                FullName = guide.FullName,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                Avatar = guide.Avatar,
                Bio = guide.Bio,
                Languages = guide.Languages,
                ExperienceYears = guide.ExperienceYears,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                IsActive = guide.Status == GuideStatus.Active && !guide.IsDeleted,  // ✅ Fixed
                Status = guide.Status.ToString(),  // ✅ Thêm status string
                Tours = tourGuides.Select(tg => new GuideTourDto
                {
                    TourId = tg.TourId,
                    TourName = tg.Tour?.Name ?? "",
                    TourImage = tg.Tour?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                    TourRating = tg.Tour?.AverageRating ?? 0,
                    IsDefault = tg.IsDefault
                }).ToList()
            };
        }

        public async Task<List<GuideListResponse>> GetActiveGuidesAsync()
        {
            var guides = await _guideRepository.GetAllAsync();

            // ✅ Fixed: Lọc theo Status = Active AND IsDeleted = false
            var activeGuides = guides.Where(g =>
                g.Status == GuideStatus.Active &&
                !g.IsDeleted
            ).ToList();

            return activeGuides.Select(MapToListResponse).ToList();
        }

        public async Task<List<GuideListResponse>> SearchGuidesAsync(
            string? keyword = null,
            string? language = null)
        {
            var guides = await _guideRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                guides = guides.Where(g =>
                    g.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    g.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(language))
            {
                guides = guides.Where(g =>
                    g.Languages != null &&
                    g.Languages.Contains(language, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return guides.Select(MapToListResponse).ToList();
        }

        private GuideListResponse MapToListResponse(Guide guide)
        {
            return new GuideListResponse
            {
                GuideId = guide.Id,
                FullName = guide.FullName,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                Avatar = guide.Avatar,
                Languages = guide.Languages,
                ExperienceYears = guide.ExperienceYears,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                IsActive = guide.Status == GuideStatus.Active && !guide.IsDeleted,  // ✅ Fixed
                Status = guide.Status.ToString()  // ✅ Thêm status string để UI hiển thị
            };
        }
    }
}
