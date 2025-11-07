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
    public class TourImageRepository : GenericRepository<TourImage>, ITourImageRepository
    {
        public TourImageRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourImage>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourImages
                .Where(i => i.TourId == tourId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<TourImage?> GetPrimaryImageAsync(int tourId)
        {
            return await _context.TourImages
                .FirstOrDefaultAsync(i => i.TourId == tourId && i.IsPrimary);
        }

        public async Task SetPrimaryImageAsync(int tourId, int imageId)
        {
            var images = await _context.TourImages
                .Where(i => i.TourId == tourId)
                .ToListAsync();

            foreach (var image in images)
            {
                image.IsPrimary = image.Id == imageId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var images = await _context.TourImages
                .Where(i => i.TourId == tourId)
                .ToListAsync();

            _context.TourImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderImagesAsync(int tourId, List<int> imageIds)
        {
            var images = await _context.TourImages
                .Where(i => i.TourId == tourId)
                .ToListAsync();

            for (int i = 0; i < imageIds.Count; i++)
            {
                var image = images.FirstOrDefault(img => img.Id == imageIds[i]);
                if (image != null)
                {
                    image.DisplayOrder = i;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
