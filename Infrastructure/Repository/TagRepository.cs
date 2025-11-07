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
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int take = 20)
        {
            // Lấy tag theo số lượng tour sử dụng
            return await _context.Tags
                .Include(t => t.TourTags)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.TourTags.Count)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower() && !t.IsDeleted);
        }

        public async Task<IEnumerable<Tag>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Tags
                .Where(t => ids.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<Tag?> GetBySlugAsync(string slug)
        {
            return await _context.Tags
                .Include(t => t.TourTags)
                    .ThenInclude(tt => tt.Tour)
                        .ThenInclude(tour => tour.Images.Where(i => i.IsPrimary))
                .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted);
        }

        public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
        {
            var query = _context.Tags.Where(t => t.Slug == slug && !t.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsWithToursAsync(int minTourCount = 1)
        {
            return await _context.Tags
                .Include(t => t.TourTags)
                .Where(t => !t.IsDeleted && t.TourTags.Count >= minTourCount)
                .OrderByDescending(t => t.TourTags.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string? keyword = null)
        {
            var query = _context.Tags.Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.Name.Contains(keyword));
            }

            return await query
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Dictionary<int, string>> GetTagDictionaryAsync()
        {
            return await _context.Tags
                .Where(t => !t.IsDeleted)
                .ToDictionaryAsync(t => t.Id, t => t.Name);
        }

        public async Task<Dictionary<int, int>> GetTagUsageCountsAsync()
        {
            return await _context.Tags
                .Include(t => t.TourTags)
                .Where(t => !t.IsDeleted)
                .ToDictionaryAsync(t => t.Id, t => t.TourTags.Count);
        }

        public async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames)
        {
            var tags = new List<Tag>();

            foreach (var tagName in tagNames.Distinct())
            {
                var tag = await GetByNameAsync(tagName);

                if (tag == null)
                {
                    // Create new tag
                    tag = new Tag
                    {
                        Name = tagName,
                        Slug = GenerateSlug(tagName),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await AddAsync(tag);
                }

                tags.Add(tag);
            }

            return tags;
        }

        public async Task<int> GetTourCountByTagAsync(int tagId)
        {
            return await _context.TourTags
                .CountAsync(tt => tt.TagId == tagId);
        }

        public async Task<IEnumerable<Tag>> GetAllTagsWithCountAsync()
        {
            return await _context.Tags
                .Include(t => t.TourTags)
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        private string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("đ", "d")
                .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");

            return System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        }
    }
}
