using Application.Interfaces;
using Application.Request.Guid;
using Application.Response.Guide;
using Application.Response.User;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserRepository _userRepository;

        public GuideService(
            IGuideRepository guideRepository,
            ITourGuideRepository tourGuideRepository,
            IUserRepository userRepository)
        {
            _guideRepository = guideRepository;
            _tourGuideRepository = tourGuideRepository;
            _userRepository = userRepository;
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
                UserId = guide.UserId, 
                FullName = guide.FullName,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                Avatar = guide.Avatar,
                Bio = guide.Bio,
                Languages = guide.Languages,
                ExperienceYears = guide.ExperienceYears,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                IsActive = guide.Status == GuideStatus.Active && !guide.IsDeleted, 
                Status = guide.Status.ToString(), 
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
        public async Task<BaseResponse<GuideProfileResponse>> UpdateProfileAsync(int userId, UpdateGuideProfileRequest request)
        {
            try
            {
                // Get guide by user ID
                var guide = await _guideRepository.GetByUserIdAsync(userId);
                if (guide == null)
                {
                    return new BaseResponse<GuideProfileResponse>
                    {
                        Success = false,
                        Message = "Guide profile not found"
                    };
                }

                // Get user to update user table as well
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<GuideProfileResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update Guide fields
                if (!string.IsNullOrEmpty(request.FullName))
                {
                    guide.FullName = request.FullName.Trim();
                    user.FullName = request.FullName.Trim();
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    guide.Email = request.Email.Trim();
                    user.Email = request.Email.Trim();
                }

                if (request.PhoneNumber != null)
                {
                    guide.PhoneNumber = request.PhoneNumber.Trim();
                    user.PhoneNumber = request.PhoneNumber.Trim();
                }

                if (request.Avatar != null)
                {
                    guide.Avatar = request.Avatar;
                    user.Avatar = request.Avatar;
                }

                if (request.Bio != null)
                {
                    guide.Bio = request.Bio.Trim();
                }

                if (request.Languages != null)
                {
                    guide.Languages = request.Languages.Trim();
                }

                if (request.ExperienceYears.HasValue)
                {
                    guide.ExperienceYears = request.ExperienceYears.Value;
                }

                await _guideRepository.UpdateAsync(guide);
                await _userRepository.UpdateAsync(user);

                var updatedGuide = await _guideRepository.GetByIdAsync(guide.Id);

                return new BaseResponse<GuideProfileResponse>
                {
                    Success = true,
                    Message = "Guide profile updated successfully",
                    Data = MapToResponse(updatedGuide)
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<GuideProfileResponse>
                {
                    Success = false,
                    Message = $"Error updating guide profile: {ex.Message}"
                };
            }
        }
        private GuideProfileResponse MapToResponse(Domain.Entities.Guide guide)
        {
            return new GuideProfileResponse
            {
                Id = guide.Id,
                UserId = guide.UserId ?? 0,
                FullName = guide.FullName,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                Avatar = guide.Avatar,
                Bio = guide.Bio,
                Languages = guide.Languages,
                ExperienceYears = guide.ExperienceYears,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                Status = guide.Status.ToString(),
                CreatedAt = guide.CreatedAt,
                UpdatedAt = guide.UpdatedAt
            };
        }
        public async Task<GuideDetailResponse> GetGuideByUserIdAsync(int userId)
        {
            var guide = await _guideRepository.GetByUserIdAsync(userId);

            if (guide == null)
                return null;

            return new GuideDetailResponse
            {
                GuideId = guide.Id,
                UserId = guide.UserId,
                FullName = guide.User?.FullName ?? "N/A",
                Email = guide.User?.Email ?? "N/A",
                PhoneNumber = guide.User?.PhoneNumber ?? "N/A",
                Avatar = guide.User?.Avatar,
                Bio = guide.Bio,
                Languages = guide.Languages,
                ExperienceYears = guide.ExperienceYears,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                Tours = guide.TourGuides?
                    .Where(tg => tg.Tour != null && !tg.Tour.IsDeleted)
                    .Select(tg => new GuideTourDto
                    {
                        TourId = tg.Tour.Id,
                        TourName = tg.Tour.Name,
                       
                    })
                    .ToList() ?? new List<GuideTourDto>()
            };
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
                IsActive = guide.Status == GuideStatus.Active && !guide.IsDeleted, 
                Status = guide.Status.ToString() 
            };
        }
    }
}
