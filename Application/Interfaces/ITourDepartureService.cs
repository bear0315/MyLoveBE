using Application.Request.Booking;
using Application.Response.Tour;
using Application.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITourDepartureService
    {
        Task<BaseResponse<TourDepartureResponse>> GetDepartureByIdAsync(int id);
        Task<DepartureListResponse> GetAllDeparturesAsync(int tourId);
        Task<DepartureListResponse> GetAvailableDeparturesAsync(int tourId, DateTime? fromDate = null);
        Task<BaseResponse<TourDepartureResponse>> CreateDepartureAsync(int tourId, CreateDepartureDto request);
        Task<DepartureListResponse> BulkCreateDeparturesAsync(int tourId, List<CreateDepartureDto> requests);
        Task<DepartureListResponse> GenerateDeparturesAsync(BulkCreateDeparturesRequest request);
        Task<BaseResponse<TourDepartureResponse>> UpdateDepartureAsync(int departureId, CreateDepartureDto request);
        Task<BaseResponse> DeleteDepartureAsync(int departureId);
        Task<bool> CheckAvailabilityAsync(int departureId, int numberOfGuests);
        Task UpdateDepartureStatusAsync(int departureId);
    }
}
