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
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .Include(f => f.Tour)
                    .ThenInclude(t => t.Images.Where(i => i.IsPrimary))
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tour>> GetFavoriteToursByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Favorites
                .Include(f => f.Tour)
                    .ThenInclude(t => t.Destination)
                .Include(f => f.Tour)
                    .ThenInclude(t => t.Images.Where(i => i.IsPrimary))
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Tour)
                .ToListAsync();
        }

        public async Task<bool> IsFavoriteAsync(int userId, int tourId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.TourId == tourId);
        }

        public async Task<Favorite?> GetByUserAndTourAsync(int userId, int tourId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TourId == tourId);
        }

        public async Task ToggleFavoriteAsync(int userId, int tourId)
        {
            var favorite = await GetByUserAndTourAsync(userId, tourId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
            }
            else
            {
                await _context.Favorites.AddAsync(new Favorite
                {
                    UserId = userId,
                    TourId = tourId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByTourIdAsync(int tourId)
        {
            return await _context.Favorites.CountAsync(f => f.TourId == tourId);
        }
    }
}
