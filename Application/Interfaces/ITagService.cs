using Application.Request.Tag;
using Application.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITagService
    {
        Task<TagDetailResponse?> GetTagByIdAsync(int id);
        Task<TagDetailResponse?> GetTagBySlugAsync(string slug);
        Task<List<TagResponse>> GetAllTagsAsync();
        Task<List<TagResponse>> GetPopularTagsAsync(int take = 20);
        Task<List<TagResponse>> GetTagsWithToursAsync(int minTourCount = 1);
        Task<List<TagResponse>> SearchTagsAsync(string? keyword = null);
        Task<Dictionary<int, string>> GetTagDictionaryAsync();
        Task<Dictionary<int, int>> GetTagUsageCountsAsync();
        Task<TagDetailResponse> CreateTagAsync(CreateTagRequest request);
        Task<TagDetailResponse> UpdateTagAsync(int id, UpdateTagRequest request);
        Task<bool> DeleteTagAsync(int id);
        Task<List<TagResponse>> GetOrCreateTagsAsync(List<string> tagNames);
        string GenerateSlug(string name);
    }
}
