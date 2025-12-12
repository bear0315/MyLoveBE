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
    public class PointsHistoryRepository : IPointsHistoryRepository
    {
        private readonly DatabaseContext _context;

        public PointsHistoryRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<PointsHistory> CreateAsync(PointsHistory history)
        {
            history.CreatedAt = DateTime.UtcNow;
            _context.PointsHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<List<PointsHistory>> GetByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _context.PointsHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _context.PointsHistories
                .Where(h => h.UserId == userId)
                .CountAsync();
        }
    }
}
