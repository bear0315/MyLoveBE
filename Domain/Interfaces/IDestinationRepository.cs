using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDestinationRepository : IGenericRepository<Destination>
    {
        Task<IEnumerable<Destination>> GetActiveDestinationsAsync();
        Task<Destination?> GetBySlugAsync(string slug);
        Task<IEnumerable<Destination>> GetPopularDestinationsAsync(int take = 10);
    }
}
