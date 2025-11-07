using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourItineraryRepository : IGenericRepository<TourItinerary>
    {
        Task<IEnumerable<TourItinerary>> GetByTourIdAsync(int tourId);
        Task<TourItinerary?> GetByTourAndDayAsync(int tourId, int dayNumber);
        Task BulkDeleteByTourIdAsync(int tourId);
        Task BulkAddAsync(List<TourItinerary> itineraries);
    }
}
