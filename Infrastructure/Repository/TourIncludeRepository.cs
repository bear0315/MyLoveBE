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
    public class TourIncludeRepository : GenericRepository<TourInclude>, ITourIncludeRepository
    {
        public TourIncludeRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourInclude>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourIncludes
                .Where(i => i.TourId == tourId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var includes = await _context.TourIncludes
                .Where(i => i.TourId == tourId)
                .ToListAsync();

            _context.TourIncludes.RemoveRange(includes);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(List<TourInclude> includes)
        {
            await _context.TourIncludes.AddRangeAsync(includes);
            await _context.SaveChangesAsync();
        }
    }
}
