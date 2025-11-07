using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourTagRepository : IGenericRepository<TourTag>
    {
        Task<IEnumerable<TourTag>> GetByTourIdAsync(int tourId);
        Task<IEnumerable<TourTag>> GetByTagIdAsync(int tagId);
        Task BulkDeleteByTourIdAsync(int tourId);
        Task BulkAddAsync(List<TourTag> tourTags);
        Task<IEnumerable<int>> GetTourIdsByTagsAsync(List<int> tagIds);
    }
}
