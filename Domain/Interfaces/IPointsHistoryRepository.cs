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

        Task<int> GetTotalPointsByUserIdAsync(int userId);
        Task<List<PointsHistory>> GetAllByUserIdAsync(int userId);
        IQueryable<PointsHistory> GetAll();
    }
}
