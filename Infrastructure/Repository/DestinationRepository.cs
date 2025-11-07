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
    public class DestinationRepository : GenericRepository<Destination>, IDestinationRepository
    {
        public DestinationRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Destination>> GetActiveDestinationsAsync()
        {
            return await _context.Destinations
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Destination?> GetBySlugAsync(string slug)
        {
            return await _context.Destinations
                .Include(d => d.Tours.Where(t => t.Status == Domain.Entities.Enums.TourStatus.Active))
                    .ThenInclude(t => t.Images.Where(i => i.IsPrimary))
                .FirstOrDefaultAsync(d => d.Slug == slug && !d.IsDeleted);
        }

        public async Task<IEnumerable<Destination>> GetPopularDestinationsAsync(int take = 10)
        {
            return await _context.Destinations
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.TourCount)
                .ThenByDescending(d => d.AverageRating)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetFeaturedDestinationsAsync(int take = 10)
        {
            return await _context.Destinations
                .Where(d => d.IsFeatured && !d.IsDeleted)
                .OrderBy(d => d.DisplayOrder)
                .ThenByDescending(d => d.AverageRating)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
        {
            var query = _context.Destinations.Where(d => d.Slug == slug && !d.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Destination>> GetDestinationsWithToursAsync()
        {
            return await _context.Destinations
                .Include(d => d.Tours.Where(t => t.Status == Domain.Entities.Enums.TourStatus.Active))
                    .ThenInclude(t => t.Images.Where(i => i.IsPrimary))
                .Where(d => !d.IsDeleted && d.TourCount > 0)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Destination> Destinations, int TotalCount)> SearchDestinationsAsync(
            string? keyword = null,
            string? country = null,
            bool? isFeatured = null,
            decimal? minRating = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "name",
            bool sortDesc = false)
        {
            var query = _context.Destinations.Where(d => !d.IsDeleted);

            // Keyword search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.Name.Contains(keyword) ||
                    (d.Description != null && d.Description.Contains(keyword)) ||
                    d.Country.Contains(keyword));
            }

            // Filter by country
            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(d => d.Country == country);
            }

            // Filter by featured
            if (isFeatured.HasValue)
            {
                query = query.Where(d => d.IsFeatured == isFeatured.Value);
            }

            // Filter by rating
            if (minRating.HasValue)
            {
                query = query.Where(d => d.AverageRating >= minRating.Value);
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortDesc ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
                "rating" => sortDesc ? query.OrderByDescending(d => d.AverageRating) : query.OrderBy(d => d.AverageRating),
                "tourcount" => sortDesc ? query.OrderByDescending(d => d.TourCount) : query.OrderBy(d => d.TourCount),
                "price" => sortDesc ? query.OrderByDescending(d => d.StartingPrice) : query.OrderBy(d => d.StartingPrice),
                "order" => sortDesc ? query.OrderByDescending(d => d.DisplayOrder) : query.OrderBy(d => d.DisplayOrder),
                _ => query.OrderBy(d => d.DisplayOrder).ThenBy(d => d.Name)
            };

            var destinations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (destinations, totalCount);
        }

        public async Task<int> GetTotalToursCountAsync(int destinationId)
        {
            return await _context.Tours
                .CountAsync(t => t.DestinationId == destinationId &&
                               t.Status == Domain.Entities.Enums.TourStatus.Active);
        }

        public async Task UpdateDestinationStatisticsAsync(int destinationId)
        {
            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return;

            // Update tour count
            destination.TourCount = await _context.Tours
                .CountAsync(t => t.DestinationId == destinationId &&
                               t.Status == Domain.Entities.Enums.TourStatus.Active);

            // Update starting price
            var minPrice = await _context.Tours
                .Where(t => t.DestinationId == destinationId &&
                           t.Status == Domain.Entities.Enums.TourStatus.Active)
                .MinAsync(t => (decimal?)t.Price);
            destination.StartingPrice = minPrice ?? 0;

            // Update average rating
            var tours = await _context.Tours
                .Where(t => t.DestinationId == destinationId &&
                           t.Status == Domain.Entities.Enums.TourStatus.Active &&
                           t.TotalReviews > 0)
                .ToListAsync();

            if (tours.Any())
            {
                destination.AverageRating = tours.Average(t => t.AverageRating);
                destination.TotalReviews = tours.Sum(t => t.TotalReviews);
            }
            else
            {
                destination.AverageRating = 0;
                destination.TotalReviews = 0;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetAllCountriesAsync()
        {
            return await _context.Destinations
                .Where(d => !d.IsDeleted)
                .Select(d => d.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task UpdateDisplayOrderAsync(Dictionary<int, int> displayOrders)
        {
            foreach (var kvp in displayOrders)
            {
                var destination = await _context.Destinations.FindAsync(kvp.Key);
                if (destination != null)
                {
                    destination.DisplayOrder = kvp.Value;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ToggleFeaturedAsync(int destinationId)
        {
            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return false;

            destination.IsFeatured = !destination.IsFeatured;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
