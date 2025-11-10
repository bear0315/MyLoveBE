using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReviewImageRepository : IGenericRepository<ReviewImage>
    {
        Task<IEnumerable<ReviewImage>> GetByReviewIdAsync(int reviewId);
        Task BulkAddAsync(List<ReviewImage> images);
        Task BulkDeleteByReviewIdAsync(int reviewId);
    }
}
