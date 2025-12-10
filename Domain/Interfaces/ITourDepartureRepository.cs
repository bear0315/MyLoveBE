using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourDepartureRepository
    {
        Task<TourDeparture?> GetByIdAsync(int id);
        Task<TourDeparture?> GetByIdWithDetailsAsync(int id);
        Task<List<TourDeparture>> GetByTourIdAsync(int tourId);
        Task<List<TourDeparture>> GetAvailableDeparturesAsync(int tourId, DateTime? fromDate = null);
        Task<TourDeparture?> GetByTourAndDateAsync(int tourId, DateTime date);
        Task<TourDeparture> CreateAsync(TourDeparture departure);
        Task<TourDeparture> UpdateAsync(TourDeparture departure);
        Task<bool> DeleteAsync(int id);
        Task<bool> CheckAvailabilityAsync(int departureId, int numberOfGuests);
        Task UpdateBookedGuestsAsync(int departureId);
        Task<List<TourDeparture>> BulkCreateAsync(List<TourDeparture> departures);
    }
}
