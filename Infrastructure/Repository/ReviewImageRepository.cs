using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class ReviewImageRepository : GenericRepository<ReviewImage>, IReviewImageRepository
    {
        public ReviewImageRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<ReviewImage>> GetByReviewIdAsync(int reviewId)
        {
            return await _context.ReviewImages
                .Where(i => i.ReviewId == reviewId)
                .ToListAsync();
        }

        public async Task BulkAddAsync(List<ReviewImage> images)
        {
            await _context.ReviewImages.AddRangeAsync(images);
            await _context.SaveChangesAsync();
        }

        public async Task BulkDeleteByReviewIdAsync(int reviewId)
        {
            var images = await _context.ReviewImages
                .Where(i => i.ReviewId == reviewId)
                .ToListAsync();

            _context.ReviewImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }
    }
}
