using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITourIncludeRepository : IGenericRepository<TourInclude>
    {
        Task<IEnumerable<TourInclude>> GetByTourIdAsync(int tourId);
        Task BulkDeleteByTourIdAsync(int tourId);
        Task BulkAddAsync(List<TourInclude> includes);
    }
}
