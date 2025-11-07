using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourGuideRepository : IGenericRepository<TourGuide>
    {
        Task<IEnumerable<TourGuide>> GetByTourIdAsync(int tourId);
        Task<IEnumerable<TourGuide>> GetByGuideIdAsync(int guideId);
        Task<TourGuide?> GetDefaultGuideForTourAsync(int tourId);
        Task BulkDeleteByTourIdAsync(int tourId);
        Task BulkAddAsync(List<TourGuide> tourGuides);
        Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate);
    }
}
