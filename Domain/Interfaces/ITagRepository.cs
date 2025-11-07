using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int take = 20);
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetByIdsAsync(List<int> ids);
    }
}
