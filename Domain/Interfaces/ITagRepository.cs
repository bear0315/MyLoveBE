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
        Task<Tag?> GetBySlugAsync(string slug);
        Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null);
        Task<IEnumerable<Tag>> GetTagsWithToursAsync(int minTourCount = 1);
        Task<IEnumerable<Tag>> SearchTagsAsync(string? keyword = null);
        Task<Dictionary<int, string>> GetTagDictionaryAsync();
        Task<Dictionary<int, int>> GetTagUsageCountsAsync();
        Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames);
        Task<int> GetTourCountByTagAsync(int tagId);
        Task<IEnumerable<Tag>> GetAllTagsWithCountAsync();
    }
}
