using Application.Interfaces;
using Application.Request.Booking;
using Application.Response.Tour;
using Application.Response.User;
using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TourDepartureService : ITourDepartureService
    {
        private readonly ITourDepartureRepository _departureRepository;
        private readonly ITourRepository _tourRepository;
        private readonly IGuideRepository _guideRepository;
        private readonly ILogger<TourDepartureService> _logger;

        public TourDepartureService(
            ITourDepartureRepository departureRepository,
            ITourRepository tourRepository,
            IGuideRepository guideRepository,
            ILogger<TourDepartureService> logger)
        {
            _departureRepository = departureRepository;
            _tourRepository = tourRepository;
            _guideRepository = guideRepository;
            _logger = logger;
        }

        public async Task<BaseResponse<TourDepartureResponse>> GetDepartureByIdAsync(int id)
        {
            try
            {
                var departure = await _departureRepository.GetByIdWithDetailsAsync(id);

                if (departure == null)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = "Departure not found"
                    };
                }

                return new BaseResponse<TourDepartureResponse>
                {
                    Success = true,
                    Message = "Departure retrieved successfully",
                    Data = MapToResponse(departure)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departure {Id}", id);
                return new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = $"Error retrieving departure: {ex.Message}"
                };
            }
        }

        public async Task<DepartureListResponse> GetAllDeparturesAsync(int tourId)
        {
            try
            {
                var departures = await _departureRepository.GetByTourIdAsync(tourId);

                return new DepartureListResponse
                {
                    Success = true,
                    Message = "Departures retrieved successfully",
                    Data = departures.Select(MapToResponse).ToList(),
                    TotalCount = departures.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departures for tour {TourId}", tourId);
                return new DepartureListResponse
                {
                    Success = false,
                    Message = $"Error retrieving departures: {ex.Message}",
                    Data = new List<TourDepartureResponse>()
                };
            }
        }

        public async Task<DepartureListResponse> GetAvailableDeparturesAsync(int tourId, DateTime? fromDate = null)
        {
            try
            {
                var departures = await _departureRepository.GetAvailableDeparturesAsync(
                    tourId,
                    fromDate ?? DateTime.UtcNow);

                return new DepartureListResponse
                {
                    Success = true,
                    Message = $"Found {departures.Count} available departure(s)",
                    Data = departures.Select(MapToResponse).ToList(),
                    TotalCount = departures.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available departures for tour {TourId}", tourId);
                return new DepartureListResponse
                {
                    Success = false,
                    Message = $"Error retrieving departures: {ex.Message}",
                    Data = new List<TourDepartureResponse>()
                };
            }
        }

        public async Task<BaseResponse<TourDepartureResponse>> CreateDepartureAsync(
            int tourId,
            CreateDepartureDto request)
        {
            try
            {
                // Validate tour exists
                var tour = await _tourRepository.GetByIdAsync(tourId);
                if (tour == null)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = "Tour not found"
                    };
                }

                // Validate departure date is in the future
                if (request.DepartureDate.Date < DateTime.UtcNow.Date)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = "Departure date must be in the future"
                    };
                }

                // Validate guide if specified
                if (request.DefaultGuideId.HasValue)
                {
                    var guide = await _guideRepository.GetByIdAsync(request.DefaultGuideId.Value);
                    if (guide == null)
                    {
                        return new BaseResponse<TourDepartureResponse>
                        {
                            Success = false,
                            Message = "Guide not found"
                        };
                    }
                }

                // Check if departure already exists for this date
                var existing = await _departureRepository.GetByTourAndDateAsync(
                    tourId,
                    request.DepartureDate);

                if (existing != null)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = $"A departure already exists for {request.DepartureDate:yyyy-MM-dd}"
                    };
                }

                // Create departure
                var departure = new TourDeparture
                {
                    TourId = tourId,
                    DepartureDate = request.DepartureDate.Date,
                    EndDate = request.DepartureDate.Date.AddDays(tour.DurationDays),
                    MaxGuests = request.MaxGuests ?? tour.MaxGuests,
                    BookedGuests = 0,
                    SpecialPrice = request.SpecialPrice,
                    Status = DepartureStatus.Available,
                    Notes = request.Notes,
                    DefaultGuideId = request.DefaultGuideId
                };

                var created = await _departureRepository.CreateAsync(departure);

                // Load with details
                var departureWithDetails = await _departureRepository.GetByIdWithDetailsAsync(created.Id);

                return new BaseResponse<TourDepartureResponse>
                {
                    Success = true,
                    Message = "Departure created successfully",
                    Data = MapToResponse(departureWithDetails!)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating departure for tour {TourId}", tourId);
                return new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = $"Error creating departure: {ex.Message}"
                };
            }
        }

        public async Task<DepartureListResponse> BulkCreateDeparturesAsync(
            int tourId,
            List<CreateDepartureDto> requests)
        {
            try
            {
                var tour = await _tourRepository.GetByIdAsync(tourId);
                if (tour == null)
                {
                    return new DepartureListResponse
                    {
                        Success = false,
                        Message = "Tour not found"
                    };
                }

                var departures = new List<TourDeparture>();
                var errors = new List<string>();

                foreach (var request in requests)
                {
                    // Validate date
                    if (request.DepartureDate.Date < DateTime.UtcNow.Date)
                    {
                        errors.Add($"Skipped {request.DepartureDate:yyyy-MM-dd}: Date must be in the future");
                        continue;
                    }

                    // Check existing
                    var existing = await _departureRepository.GetByTourAndDateAsync(
                        tourId,
                        request.DepartureDate);

                    if (existing != null)
                    {
                        errors.Add($"Skipped {request.DepartureDate:yyyy-MM-dd}: Already exists");
                        continue;
                    }

                    departures.Add(new TourDeparture
                    {
                        TourId = tourId,
                        DepartureDate = request.DepartureDate.Date,
                        EndDate = request.DepartureDate.Date.AddDays(tour.DurationDays),
                        MaxGuests = request.MaxGuests ?? tour.MaxGuests,
                        BookedGuests = 0,
                        SpecialPrice = request.SpecialPrice,
                        Status = DepartureStatus.Available,
                        Notes = request.Notes,
                        DefaultGuideId = request.DefaultGuideId
                    });
                }

                if (!departures.Any())
                {
                    return new DepartureListResponse
                    {
                        Success = false,
                        Message = "No valid departures to create. Errors: " + string.Join("; ", errors)
                    };
                }

                var created = await _departureRepository.BulkCreateAsync(departures);

                var message = $"Created {created.Count} departure(s) successfully";
                if (errors.Any())
                {
                    message += $". {errors.Count} skipped: {string.Join("; ", errors)}";
                }

                return new DepartureListResponse
                {
                    Success = true,
                    Message = message,
                    Data = created.Select(MapToResponse).ToList(),
                    TotalCount = created.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating departures");
                return new DepartureListResponse
                {
                    Success = false,
                    Message = $"Error creating departures: {ex.Message}"
                };
            }
        }

        public async Task<DepartureListResponse> GenerateDeparturesAsync(BulkCreateDeparturesRequest request)
        {
            try
            {
                var tour = await _tourRepository.GetByIdAsync(request.TourId);
                if (tour == null)
                {
                    return new DepartureListResponse
                    {
                        Success = false,
                        Message = "Tour not found"
                    };
                }

                var departureDates = GenerateDepartureDates(request);

                if (!departureDates.Any())
                {
                    return new DepartureListResponse
                    {
                        Success = false,
                        Message = "No departure dates generated"
                    };
                }

                // Create DTOs from generated dates
                var createRequests = departureDates.Select(date => new CreateDepartureDto
                {
                    DepartureDate = date,
                    MaxGuests = request.MaxGuestsOverride
                }).ToList();

                return await BulkCreateDeparturesAsync(request.TourId, createRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating departures");
                return new DepartureListResponse
                {
                    Success = false,
                    Message = $"Error generating departures: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<TourDepartureResponse>> UpdateDepartureAsync(
            int departureId,
            CreateDepartureDto request)
        {
            try
            {
                var departure = await _departureRepository.GetByIdAsync(departureId);
                if (departure == null)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = "Departure not found"
                    };
                }

                // Cannot update if already has bookings
                if (departure.BookedGuests > 0)
                {
                    return new BaseResponse<TourDepartureResponse>
                    {
                        Success = false,
                        Message = "Cannot update departure that already has bookings"
                    };
                }

                // Validate guide if changed
                if (request.DefaultGuideId.HasValue &&
                    request.DefaultGuideId != departure.DefaultGuideId)
                {
                    var guide = await _guideRepository.GetByIdAsync(request.DefaultGuideId.Value);
                    if (guide == null)
                    {
                        return new BaseResponse<TourDepartureResponse>
                        {
                            Success = false,
                            Message = "Guide not found"
                        };
                    }
                }

                // Update fields
                departure.DepartureDate = request.DepartureDate.Date;

                var tour = await _tourRepository.GetByIdAsync(departure.TourId);
                departure.EndDate = request.DepartureDate.Date.AddDays(tour!.DurationDays);

                if (request.MaxGuests.HasValue)
                    departure.MaxGuests = request.MaxGuests.Value;

                departure.SpecialPrice = request.SpecialPrice;
                departure.Notes = request.Notes;
                departure.DefaultGuideId = request.DefaultGuideId;

                var updated = await _departureRepository.UpdateAsync(departure);

                return new BaseResponse<TourDepartureResponse>
                {
                    Success = true,
                    Message = "Departure updated successfully",
                    Data = MapToResponse(updated)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating departure {Id}", departureId);
                return new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = $"Error updating departure: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> DeleteDepartureAsync(int departureId)
        {
            try
            {
                var departure = await _departureRepository.GetByIdAsync(departureId);
                if (departure == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Departure not found"
                    };
                }

                // Cannot delete if has bookings
                if (departure.BookedGuests > 0)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Cannot delete departure that has bookings. Cancel the departure instead."
                    };
                }

                var result = await _departureRepository.DeleteAsync(departureId);

                return new BaseResponse
                {
                    Success = result,
                    Message = result ? "Departure deleted successfully" : "Failed to delete departure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting departure {Id}", departureId);
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Error deleting departure: {ex.Message}"
                };
            }
        }

        public async Task<bool> CheckAvailabilityAsync(int departureId, int numberOfGuests)
        {
            try
            {
                return await _departureRepository.CheckAvailabilityAsync(departureId, numberOfGuests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for departure {Id}", departureId);
                return false;
            }
        }

        public async Task UpdateDepartureStatusAsync(int departureId)
        {
            try
            {
                await _departureRepository.UpdateBookedGuestsAsync(departureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating departure status {Id}", departureId);
            }
        }

        #region Helper Methods

        private TourDepartureResponse MapToResponse(TourDeparture departure)
        {
            var effectivePrice = departure.SpecialPrice ?? departure.Tour?.Price ?? 0;

            return new TourDepartureResponse
            {
                Id = departure.Id,
                TourId = departure.TourId,
                TourName = departure.Tour?.Name ?? "",
                DepartureDate = departure.DepartureDate,
                EndDate = departure.EndDate,
                MaxGuests = departure.MaxGuests,
                BookedGuests = departure.BookedGuests,
                AvailableSlots = departure.AvailableSlots,
                Price = effectivePrice,
                HasSpecialPrice = departure.SpecialPrice.HasValue,
                Status = departure.Status.ToString(),
                Notes = departure.Notes,
                DefaultGuideId = departure.DefaultGuideId,
                DefaultGuideName = departure.DefaultGuide?.FullName ??
                                  departure.DefaultGuide?.User?.FullName
            };
        }

        private List<DateTime> GenerateDepartureDates(BulkCreateDeparturesRequest request)
        {
            var dates = new List<DateTime>();
            var current = request.StartDate.Date;

            switch (request.Pattern.ToLower())
            {
                case "daily":
                    while (current <= request.EndDate.Date)
                    {
                        if (current >= DateTime.UtcNow.Date)
                        {
                            dates.Add(current);
                        }
                        current = current.AddDays(1);
                    }
                    break;

                case "weekly":
                    if (request.DaysOfWeek == null || !request.DaysOfWeek.Any())
                    {
                        // Default: every week on start date's day
                        while (current <= request.EndDate.Date)
                        {
                            if (current >= DateTime.UtcNow.Date)
                            {
                                dates.Add(current);
                            }
                            current = current.AddDays(7);
                        }
                    }
                    else
                    {
                        // Specific days of week
                        var targetDays = request.DaysOfWeek
                            .Select(d => Enum.Parse<DayOfWeek>(d, true))
                            .ToHashSet();

                        while (current <= request.EndDate.Date)
                        {
                            if (targetDays.Contains(current.DayOfWeek) &&
                                current >= DateTime.UtcNow.Date)
                            {
                                dates.Add(current);
                            }
                            current = current.AddDays(1);
                        }
                    }
                    break;

                case "biweekly":
                    while (current <= request.EndDate.Date)
                    {
                        if (current >= DateTime.UtcNow.Date)
                        {
                            dates.Add(current);
                        }
                        current = current.AddDays(14);
                    }
                    break;

                case "monthly":
                    var dayOfMonth = current.Day;
                    while (current <= request.EndDate.Date)
                    {
                        if (current >= DateTime.UtcNow.Date)
                        {
                            dates.Add(current);
                        }

                        // Next month, same day
                        var nextMonth = current.AddMonths(1);
                        var daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                        var targetDay = Math.Min(dayOfMonth, daysInNextMonth);
                        current = new DateTime(nextMonth.Year, nextMonth.Month, targetDay);
                    }
                    break;

                default:
                    throw new ArgumentException($"Invalid pattern: {request.Pattern}");
            }

            return dates;
        }

        #endregion
    }
}
