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
    public class TourTagRepository : GenericRepository<TourTag>, ITourTagRepository
    {
        public TourTagRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourTag>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourTags
                .Include(tt => tt.Tag)
                .Where(tt => tt.TourId == tourId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourTag>> GetByTagIdAsync(int tagId)
        {
            return await _context.TourTags
                .Include(tt => tt.Tour)
                .Where(tt => tt.TagId == tagId)
                .ToListAsync();
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var tourTags = await _context.TourTags
                .Where(tt => tt.TourId == tourId)
                .ToListAsync();

            _context.TourTags.RemoveRange(tourTags);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(List<TourTag> tourTags)
        {
            await _context.TourTags.AddRangeAsync(tourTags);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<int>> GetTourIdsByTagsAsync(List<int> tagIds)
        {
            return await _context.TourTags
                .Where(tt => tagIds.Contains(tt.TagId))
                .Select(tt => tt.TourId)
                .Distinct()
                .ToListAsync();
        }
    }
}
