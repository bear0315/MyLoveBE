using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourImageRepository : IGenericRepository<TourImage>
    {
        Task<IEnumerable<TourImage>> GetByTourIdAsync(int tourId);
        Task<TourImage?> GetPrimaryImageAsync(int tourId);
        Task SetPrimaryImageAsync(int tourId, int imageId);
        Task BulkDeleteByTourIdAsync(int tourId);
        Task ReorderImagesAsync(int tourId, List<int> imageIds);
    }
}
