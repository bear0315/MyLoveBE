using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPointsHistoryRepository
    {
        Task<PointsHistory> CreateAsync(PointsHistory history);
        Task<List<PointsHistory>> GetByUserIdAsync(int userId, int page, int pageSize);
        Task<int> GetCountByUserIdAsync(int userId);
    }
}
