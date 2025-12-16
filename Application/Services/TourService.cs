using Application.Interfaces;
using Application.Request.Tour;
using Application.Response.Common;
using Application.Response.Tour;
using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Response.Guide;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;
        private readonly ITourImageRepository _imageRepository;
        private readonly ITourItineraryRepository _itineraryRepository;
        private readonly ITourIncludeRepository _includeRepository;
        private readonly ITourExcludeRepository _excludeRepository;
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly ITourTagRepository _tourTagRepository;
        private readonly IGuideRepository _guideRepository;
        private readonly ITourDepartureRepository _departureRepository;

        public TourService(
            ITourRepository tourRepository,
            ITourImageRepository imageRepository,
            ITourItineraryRepository itineraryRepository,
            ITourIncludeRepository includeRepository,
            ITourExcludeRepository excludeRepository,
            ITourGuideRepository tourGuideRepository,
            ITourTagRepository tourTagRepository,
            IGuideRepository guideRepository,
            IBookingRepository bookingRepository,
            ITourDepartureRepository departureRepository)
        {
            _tourRepository = tourRepository;
            _imageRepository = imageRepository;
            _itineraryRepository = itineraryRepository;
            _includeRepository = includeRepository;
            _excludeRepository = excludeRepository;
            _tourGuideRepository = tourGuideRepository;
            _tourTagRepository = tourTagRepository;
            _guideRepository = guideRepository;
            _departureRepository = departureRepository;
        }

        public async Task<TourDetailResponse?> GetTourByIdAsync(int id)
        {
            var tour = await _tourRepository.GetTourDetailAsync(id);
            return tour == null ? null : await MapToDetailResponseAsync(tour);
        }

        public async Task<TourDetailResponse?> GetTourBySlugAsync(string slug)
        {
            var tour = await _tourRepository.GetTourBySlugAsync(slug);
            return tour == null ? null : await MapToDetailResponseAsync(tour);
        }

        public async Task<PagedResult<TourListResponse>> GetAllToursAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (tours, totalCount) = await _tourRepository.SearchToursAsync(
                pageNumber: pageNumber,
                pageSize: pageSize);

            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }

            return new PagedResult<TourListResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<TourDetailResponse> CreateTourAsync(CreateTourRequest request)
        {
            // Generate slug
            var slug = GenerateSlug(request.Name);
            var slugExists = await IsSlugExistsAsync(slug);
            if (slugExists)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            }

            // Create tour entity
            var tour = new Tour
            {
                Name = request.Name,
                Description = request.Description,
                Location = request.Location,
                Price = request.Price,
                Duration = request.Duration,
                DurationDays = request.DurationDays,
                MaxGuests = request.MaxGuests,
                Type = request.Type,
                Category = request.Category,
                Difficulty = request.Difficulty,
                IsFeatured = request.IsFeatured,
                PhysicalRequirements = request.PhysicalRequirements,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                SpecialRequirements = request.SpecialRequirements,
                Slug = slug,
                MetaTitle = request.MetaTitle ?? request.Name,
                MetaDescription = request.MetaDescription,
                Status = TourStatus.Active
            };

            var createdTour = await _tourRepository.AddAsync(tour);

            // Add related data
            if (request.Images != null && request.Images.Any())
            {
                var images = request.Images.Select(img => new TourImage
                {
                    TourId = createdTour.Id,
                    ImageUrl = img.ImageUrl,
                    Caption = img.Caption,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder
                }).ToList();

                foreach (var image in images)
                {
                    await _imageRepository.AddAsync(image);
                }
            }

            if (request.Itineraries != null && request.Itineraries.Any())
            {
                var itineraries = request.Itineraries.Select(it => new TourItinerary
                {
                    TourId = createdTour.Id,
                    DayNumber = it.DayNumber,
                    Title = it.Title,
                    Description = it.Description,
                    Activities = it.Activities,
                    Meals = it.Meals,
                    Accommodation = it.Accommodation
                }).ToList();

                await _itineraryRepository.BulkAddAsync(itineraries);
            }

            if (request.Includes != null && request.Includes.Any())
            {
                var includes = request.Includes.Select((item, index) => new TourInclude
                {
                    TourId = createdTour.Id,
                    Item = item,
                    DisplayOrder = index
                }).ToList();

                await _includeRepository.BulkAddAsync(includes);
            }

            if (request.Excludes != null && request.Excludes.Any())
            {
                var excludes = request.Excludes.Select((item, index) => new TourExclude
                {
                    TourId = createdTour.Id,
                    Item = item,
                    DisplayOrder = index
                }).ToList();

                await _excludeRepository.BulkAddAsync(excludes);
            }

            // Add Guides with validation
            if (request.GuideIds != null && request.GuideIds.Any())
            {
                var validGuides = new List<int>();
                foreach (var guideId in request.GuideIds.Distinct())
                {
                    var guide = await _guideRepository.GetByIdAsync(guideId);
                    if (guide != null)
                    {
                        validGuides.Add(guideId);
                    }
                }

                if (!validGuides.Any())
                {
                    throw new ArgumentException("No valid guides found in the provided list");
                }

                int defaultGuideId;
                if (request.DefaultGuideId.HasValue)
                {
                    if (!validGuides.Contains(request.DefaultGuideId.Value))
                    {
                        throw new ArgumentException(
                            "Default guide must be one of the selected guides for this tour");
                    }
                    defaultGuideId = request.DefaultGuideId.Value;
                }
                else
                {
                    defaultGuideId = validGuides.First();
                }

                var tourGuides = validGuides.Select(guideId => new TourGuide
                {
                    TourId = createdTour.Id,
                    GuideId = guideId,
                    IsDefault = guideId == defaultGuideId
                }).ToList();

                await _tourGuideRepository.BulkAddAsync(tourGuides);
            }

            if (request.TagIds != null && request.TagIds.Any())
            {
                var tourTags = request.TagIds.Select(tagId => new TourTag
                {
                    TourId = createdTour.Id,
                    TagId = tagId
                }).ToList();

                await _tourTagRepository.BulkAddAsync(tourTags);
            }

            var result = await GetTourByIdAsync(createdTour.Id);
            return result!;
        }

        public async Task<TourDetailResponse> UpdateTourAsync(int id, UpdateTourRequest request)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null)
                throw new KeyNotFoundException($"Tour with ID {id} not found");

            // Update slug if name changed
            if (tour.Name != request.Name)
            {
                var newSlug = GenerateSlug(request.Name);
                var slugExists = await IsSlugExistsAsync(newSlug, id);
                if (slugExists)
                {
                    newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
                }
                tour.Slug = newSlug;
            }

            // Update tour properties
            tour.Name = request.Name;
            tour.Description = request.Description;
            tour.Location = request.Location;
            tour.Price = request.Price;
            tour.Duration = request.Duration;
            tour.DurationDays = request.DurationDays;
            tour.MaxGuests = request.MaxGuests;
            tour.Type = request.Type;
            tour.Category = request.Category;
            tour.Difficulty = request.Difficulty;
            tour.IsFeatured = request.IsFeatured;
            tour.PhysicalRequirements = request.PhysicalRequirements;
            tour.MinAge = request.MinAge;
            tour.MaxAge = request.MaxAge;
            tour.SpecialRequirements = request.SpecialRequirements;
            tour.MetaTitle = request.MetaTitle ?? request.Name;
            tour.MetaDescription = request.MetaDescription;

            await _tourRepository.UpdateAsync(tour);

            // Update related data
            if (request.Images != null)
            {
                await _imageRepository.BulkDeleteByTourIdAsync(id);
                var images = request.Images.Select(img => new TourImage
                {
                    TourId = id,
                    ImageUrl = img.ImageUrl,
                    Caption = img.Caption,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder
                }).ToList();

                foreach (var image in images)
                {
                    await _imageRepository.AddAsync(image);
                }
            }

            if (request.Itineraries != null)
            {
                await _itineraryRepository.BulkDeleteByTourIdAsync(id);
                var itineraries = request.Itineraries.Select(it => new TourItinerary
                {
                    TourId = id,
                    DayNumber = it.DayNumber,
                    Title = it.Title,
                    Description = it.Description,
                    Activities = it.Activities,
                    Meals = it.Meals,
                    Accommodation = it.Accommodation
                }).ToList();

                await _itineraryRepository.BulkAddAsync(itineraries);
            }

            if (request.Includes != null)
            {
                await _includeRepository.BulkDeleteByTourIdAsync(id);
                var includes = request.Includes.Select((item, index) => new TourInclude
                {
                    TourId = id,
                    Item = item,
                    DisplayOrder = index
                }).ToList();

                await _includeRepository.BulkAddAsync(includes);
            }

            if (request.Excludes != null)
            {
                await _excludeRepository.BulkDeleteByTourIdAsync(id);
                var excludes = request.Excludes.Select((item, index) => new TourExclude
                {
                    TourId = id,
                    Item = item,
                    DisplayOrder = index
                }).ToList();

                await _excludeRepository.BulkAddAsync(excludes);
            }

            // Update Guides with validation
            if (request.GuideIds != null)
            {
                await _tourGuideRepository.BulkDeleteByTourIdAsync(id);

                if (request.GuideIds.Any())
                {
                    var validGuides = new List<int>();
                    foreach (var guideId in request.GuideIds.Distinct())
                    {
                        var guide = await _guideRepository.GetByIdAsync(guideId);
                        if (guide != null)
                        {
                            validGuides.Add(guideId);
                        }
                    }

                    if (!validGuides.Any())
                    {
                        throw new ArgumentException("No valid guides found in the provided list");
                    }

                    int defaultGuideId;
                    if (request.DefaultGuideId.HasValue)
                    {
                        if (!validGuides.Contains(request.DefaultGuideId.Value))
                        {
                            throw new ArgumentException(
                                "Default guide must be one of the selected guides for this tour");
                        }
                        defaultGuideId = request.DefaultGuideId.Value;
                    }
                    else
                    {
                        defaultGuideId = validGuides.First();
                    }

                    var tourGuides = validGuides.Select(guideId => new TourGuide
                    {
                        TourId = id,
                        GuideId = guideId,
                        IsDefault = guideId == defaultGuideId
                    }).ToList();

                    await _tourGuideRepository.BulkAddAsync(tourGuides);
                }
            }

            if (request.TagIds != null)
            {
                await _tourTagRepository.BulkDeleteByTourIdAsync(id);
                var tourTags = request.TagIds.Select(tagId => new TourTag
                {
                    TourId = id,
                    TagId = tagId
                }).ToList();

                await _tourTagRepository.BulkAddAsync(tourTags);
            }

            var result = await GetTourByIdAsync(id);
            return result!;
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null) return false;

            // Delete related data
            await _imageRepository.BulkDeleteByTourIdAsync(id);
            await _itineraryRepository.BulkDeleteByTourIdAsync(id);
            await _includeRepository.BulkDeleteByTourIdAsync(id);
            await _excludeRepository.BulkDeleteByTourIdAsync(id);
            await _tourGuideRepository.BulkDeleteByTourIdAsync(id);
            await _tourTagRepository.BulkDeleteByTourIdAsync(id);

            await _tourRepository.DeleteAsync(tour);
            return true;
        }

        public async Task<bool> ToggleFeaturedAsync(int id)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null) return false;

            tour.IsFeatured = !tour.IsFeatured;
            await _tourRepository.UpdateAsync(tour);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, TourStatus status)
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null) return false;

            tour.Status = status;
            await _tourRepository.UpdateAsync(tour);
            return true;
        }

        public async Task<PagedResult<TourListResponse>> SearchToursAsync(TourSearchRequest request)
        {
            var (tours, totalCount) = await _tourRepository.SearchToursAsync(
                keyword: request.Keyword,
                destinationId: request.DestinationId,
                category: request.Category,
                type: request.Type,
                difficulty: request.Difficulty,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                minDays: request.MinDays,
                maxDays: request.MaxDays,
                minRating: request.MinRating,
                tagIds: request.TagIds,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                sortDesc: request.SortDesc
            );

            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }

            return new PagedResult<TourListResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<TourListResponse>> GetFeaturedToursAsync(int take = 10)
        {
            var tours = await _tourRepository.GetFeaturedToursAsync(take);
            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }
            return items;
        }

        public async Task<List<TourListResponse>> GetPopularToursAsync(int take = 10)
        {
            var tours = await _tourRepository.GetPopularToursAsync(take);
            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }
            return items;
        }

        public async Task<List<TourListResponse>> GetRelatedToursAsync(int tourId, int take = 5)
        {
            var tours = await _tourRepository.GetRelatedToursAsync(tourId, take);
            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }
            return items;
        }

        public async Task<List<TourListResponse>> GetToursByDestinationAsync(int destinationId, int pageNumber = 1, int pageSize = 10)
        {
            var (tours, _) = await _tourRepository.SearchToursAsync(
                destinationId: destinationId,
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }
            return items;
        }

        public async Task<List<TourListResponse>> GetToursByCategoryAsync(TourCategory category, int pageNumber = 1, int pageSize = 10)
        {
            var (tours, _) = await _tourRepository.SearchToursAsync(
                category: category,
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var items = new List<TourListResponse>();
            foreach (var tour in tours)
            {
                items.Add(await MapToListResponseAsync(tour));
            }
            return items;
        }

        public async Task<TourStatisticsDto> GetTourStatisticsAsync()
        {
            var totalTours = await _tourRepository.CountAsync();
            var activeTours = await _tourRepository.GetTotalActiveToursAsync();

            return new TourStatisticsDto
            {
                TotalTours = totalTours,
                ActiveTours = activeTours,
            };
        }

        #region Guide Methods

        public async Task<List<AvailableGuideDto>> GetAvailableGuidesForTourAsync(int tourId, DateTime tourDate)
        {
            try
            {
                var checkDate = tourDate.Date;

                var tourGuides = await _tourGuideRepository
                    .GetAll()
                    .Where(tg => tg.TourId == tourId)
                    .Include(tg => tg.Guide)
                    .Include(tg => tg.Guide.User)
                    .ToListAsync();

                if (!tourGuides.Any())
                {
                    return new List<AvailableGuideDto>();
                }

                var result = new List<AvailableGuideDto>();

                foreach (var tg in tourGuides)
                {
                    Guide guide;
                    if (tg.Guide != null)
                    {
                        guide = tg.Guide;
                    }
                    else
                    {
                        guide = await _guideRepository.GetByIdAsync(tg.GuideId);
                        if (guide == null) continue;
                    }

                    bool isAvailable = await _tourGuideRepository.IsGuideAvailableAsync(tg.GuideId, checkDate);

                    var dto = new AvailableGuideDto
                    {
                        GuideId = tg.GuideId,
                        FullName = guide.FullName ?? guide.User?.FullName ?? "Unknown",
                        Avatar = guide.Avatar ?? guide.User?.Avatar,
                        Bio = guide.Bio,
                        Languages = guide.Languages,
                        IsAvailable = isAvailable,
                        IsDefaultGuide = tg.IsDefault,
                        AverageRating = guide.AverageRating,
                        TotalReviews = guide.TotalReviews,
                        UnavailabilityReason = !isAvailable
                            ? $"Hướng dẫn viên đã có lịch vào ngày {checkDate:dd/MM/yyyy}"
                            : null,
                    };

                    result.Add(dto);

                    Console.WriteLine($"[TourService] Guide {guide.FullName} (ID: {tg.GuideId}):");
                    Console.WriteLine($"  - IsAvailable: {isAvailable}");
                    Console.WriteLine($"  - Check Date: {checkDate:yyyy-MM-dd}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TourService] Error in GetAvailableGuidesForTourAsync: {ex.Message}");
                Console.WriteLine($"[TourService] Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<GuideDetailDto?> GetGuideDetailAsync(int guideId)
        {
            var guide = await _guideRepository.GetByIdAsync(guideId);
            if (guide == null) return null;

            var tourGuides = await _tourGuideRepository.GetByGuideIdAsync(guideId);

            return new GuideDetailDto
            {
                GuideId = guide.Id,
                FullName = guide.FullName,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                Avatar = guide.Avatar,
                Bio = guide.Bio,
                Languages = guide.Languages,
                AverageRating = guide.AverageRating,
                TotalReviews = guide.TotalReviews,
                YearsOfExperience = guide.ExperienceYears,
                Specialties = ParseLanguages(guide.Languages),
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

        public async Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate)
        {
            return await _tourGuideRepository.IsGuideAvailableAsync(guideId, tourDate);
        }

        public async Task<int?> GetDefaultGuideIdForTourAsync(int tourId, DateTime tourDate)
        {
            var defaultGuide = await _tourGuideRepository.GetDefaultGuideForTourAsync(tourId);

            if (defaultGuide != null)
            {
                var isAvailable = await _tourGuideRepository.IsGuideAvailableAsync(defaultGuide.GuideId, tourDate);
                if (isAvailable)
                {
                    return defaultGuide.GuideId;
                }
            }

            var availableGuides = await GetAvailableGuidesForTourAsync(tourId, tourDate);
            var firstAvailable = availableGuides.FirstOrDefault(g => g.IsAvailable);

            return firstAvailable?.GuideId;
        }

        #endregion

        #region Statistics & Availability Methods

        public async Task UpdateTourStatisticsAsync(int tourId)
        {
            await _tourRepository.UpdateTourStatisticsAsync(tourId);
        }

        public async Task UpdateTourRatingAsync(int tourId)
        {
            await _tourRepository.UpdateTourRatingAsync(tourId);
        }

        public async Task<bool> CheckTourAvailabilityAsync(int tourId, DateTime tourDate, int numberOfGuests)
        {
            return await _tourRepository.IsTourAvailableAsync(tourId, tourDate, numberOfGuests);
        }

        public async Task<List<DateTime>> GetAvailableDatesAsync(int tourId, DateTime fromDate, DateTime toDate)
        {
            var dates = await _tourRepository.GetAvailableDatesAsync(tourId, fromDate, toDate);
            return dates.ToList();
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkUpdateStatusAsync(BulkUpdateStatusRequest request)
        {
            return await _tourRepository.BulkUpdateStatusAsync(request.TourIds, request.Status);
        }

        public async Task<bool> BulkUpdateFeaturedAsync(List<int> tourIds, bool isFeatured)
        {
            return await _tourRepository.BulkUpdateFeaturedAsync(tourIds, isFeatured);
        }

        public async Task<bool> BulkDeleteToursAsync(List<int> tourIds)
        {
            return await _tourRepository.BulkDeleteAsync(tourIds);
        }

        #endregion

        #region Validation Methods

        public async Task<bool> IsTourExistsAsync(int id)
        {
            return await _tourRepository.ExistsAsync(id);
        }

        public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
        {
            return await _tourRepository.IsSlugExistsAsync(slug, excludeId);
        }

        public string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }

        #endregion

        #region Helper Methods

        private List<string> ParseLanguages(string? languages)
        {
            if (string.IsNullOrWhiteSpace(languages))
                return new List<string>();

            return languages.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(l => l.Trim())
                           .ToList();
        }

        #endregion

        #region Mapping Methods

        private async Task<TourListResponse> MapToListResponseAsync(Tour tour)
        {
            // Get available departures for this tour
            var departures = await _departureRepository.GetAvailableDeparturesAsync(
                tour.Id,
                DateTime.UtcNow.Date);

            return new TourListResponse
            {
                Id = tour.Id,
                Name = tour.Name,
                Slug = tour.Slug,
                Location = tour.Location,
                Price = tour.Price,
                MaxGuests = tour.MaxGuests,
                Duration = tour.Duration,
                DurationDays = tour.DurationDays,
                Type = tour.Type,
                Category = tour.Category,
                Status = tour.Status,
                IsFeatured = tour.IsFeatured,
                AverageRating = tour.AverageRating,
                TotalReviews = tour.TotalReviews,
                TotalBookings = tour.TotalBookings,
                PrimaryImageUrl = tour.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                DestinationName = tour.Destination?.Name ?? "",
                CreatedAt = tour.CreatedAt,

                // Map departures
                Departures = departures.Select(d => new TourDepartureDto
                {
                    Id = d.Id,
                    DepartureDate = d.DepartureDate,
                    EndDate = d.EndDate,
                    MaxGuests = d.MaxGuests,
                    BookedGuests = d.BookedGuests,
                    AvailableSlots = d.AvailableSlots,
                    Price = d.SpecialPrice ?? tour.Price,
                    HasSpecialPrice = d.SpecialPrice.HasValue,
                    Status = d.Status.ToString(),
                    Notes = d.Notes,
                    DefaultGuideId = d.DefaultGuideId,
                    DefaultGuideName = d.DefaultGuide?.FullName ?? d.DefaultGuide?.User?.FullName
                }).ToList()
            };
        }

        private async Task<TourDetailResponse> MapToDetailResponseAsync(Tour tour)
        {
            // Get available departures for this tour
            var departures = await _departureRepository.GetAvailableDeparturesAsync(
                tour.Id,
                DateTime.UtcNow.Date);

            return new TourDetailResponse
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                Slug = tour.Slug,
                DestinationName = tour.Destination?.Name ?? "",
                Location = tour.Location,
                Price = tour.Price,
                Duration = tour.Duration,
                DurationDays = tour.DurationDays,
                MaxGuests = tour.MaxGuests,
                Type = tour.Type,
                Category = tour.Category,
                Difficulty = tour.Difficulty,
                Status = tour.Status,
                IsFeatured = tour.IsFeatured,
                AverageRating = tour.AverageRating,
                TotalReviews = tour.TotalReviews,
                TotalBookings = tour.TotalBookings,
                TotalRevenue = tour.TotalRevenue,
                PhysicalRequirements = tour.PhysicalRequirements,
                MinAge = tour.MinAge,
                MaxAge = tour.MaxAge,
                SpecialRequirements = tour.SpecialRequirements,
                MetaTitle = tour.MetaTitle,
                MetaDescription = tour.MetaDescription,
                CreatedAt = tour.CreatedAt,

                Images = tour.Images.Select(i => new TourImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),

                Itineraries = tour.Itineraries.Select(i => new TourItineraryDto
                {
                    Id = i.Id,
                    DayNumber = i.DayNumber,
                    Title = i.Title,
                    Description = i.Description,
                    Activities = i.Activities,
                    Meals = i.Meals,
                    Accommodation = i.Accommodation
                }).ToList(),

                Includes = tour.Includes.Select(i => new TourIncludeDto
                {
                    Id = i.Id,
                    Item = i.Item,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),

                Excludes = tour.Excludes.Select(e => new TourExcludeDto
                {
                    Id = e.Id,
                    Item = e.Item,
                    DisplayOrder = e.DisplayOrder
                }).ToList(),

                Guides = tour.TourGuides.Select(tg => new TourGuideDto
                {
                    GuideId = tg.GuideId,
                    GuideName = tg.Guide?.User?.FullName ?? "",
                    GuideAvatar = tg.Guide?.User?.Avatar,
                    GuideRating = tg.Guide?.AverageRating ?? 0,
                    IsDefault = tg.IsDefault
                }).ToList(),

                Tags = tour.TourTags.Select(tt => new TagDto
                {
                    Id = tt.TagId,
                    Name = tt.Tag?.Name ?? ""
                }).ToList(),

                Reviews = tour.Reviews.Select(r => new TourReviewDto
                {
                    Id = r.Id,
                    UserName = r.User?.FullName ?? "",
                    UserAvatar = r.User?.Avatar,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList(),

                // Map departures
                Departures = departures.Select(d => new TourDepartureDto
                {
                    Id = d.Id,
                    DepartureDate = d.DepartureDate,
                    EndDate = d.EndDate,
                    MaxGuests = d.MaxGuests,
                    BookedGuests = d.BookedGuests,
                    AvailableSlots = d.AvailableSlots,
                    Price = d.SpecialPrice ?? tour.Price,
                    HasSpecialPrice = d.SpecialPrice.HasValue,
                    Status = d.Status.ToString(),
                    Notes = d.Notes,
                    DefaultGuideId = d.DefaultGuideId,
                    DefaultGuideName = d.DefaultGuide?.FullName ?? d.DefaultGuide?.User?.FullName
                }).ToList()
            };
        }

        #endregion
    }
}