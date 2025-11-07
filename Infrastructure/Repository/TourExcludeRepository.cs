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
    public class TourExcludeRepository : GenericRepository<TourExclude>, ITourExcludeRepository
    {
        public TourExcludeRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourExclude>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourExcludes
                .Where(e => e.TourId == tourId)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var excludes = await _context.TourExcludes
                .Where(e => e.TourId == tourId)
                .ToListAsync();

            _context.TourExcludes.RemoveRange(excludes);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(List<TourExclude> excludes)
        {
            await _context.TourExcludes.AddRangeAsync(excludes);
            await _context.SaveChangesAsync();
        }
    }

}
