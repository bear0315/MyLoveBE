using Application.Interfaces;
using Application.Request.Tag;
using Application.Response.Tag;
using Application.Response.Tour;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<TagDetailResponse?> GetTagByIdAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null) return null;

            var tourCount = await _tagRepository.GetTourCountByTagAsync(id);
            return MapToDetailResponse(tag, tourCount);
        }

        public async Task<TagDetailResponse?> GetTagBySlugAsync(string slug)
        {
            var tag = await _tagRepository.GetBySlugAsync(slug);
            if (tag == null) return null;

            var tourCount = tag.TourTags?.Count ?? 0;
            return MapToDetailResponse(tag, tourCount);
        }

        public async Task<List<TagResponse>> GetAllTagsAsync()
        {
            var tags = await _tagRepository.GetAllTagsWithCountAsync();
            return tags.Select(MapToResponse).ToList();
        }

        public async Task<List<TagResponse>> GetPopularTagsAsync(int take = 20)
        {
            var tags = await _tagRepository.GetPopularTagsAsync(take);
            return tags.Select(MapToResponse).ToList();
        }

        public async Task<List<TagResponse>> GetTagsWithToursAsync(int minTourCount = 1)
        {
            var tags = await _tagRepository.GetTagsWithToursAsync(minTourCount);
            return tags.Select(MapToResponse).ToList();
        }

        public async Task<List<TagResponse>> SearchTagsAsync(string? keyword = null)
        {
            var tags = await _tagRepository.SearchTagsAsync(keyword);
            var usageCounts = await _tagRepository.GetTagUsageCountsAsync();

            return tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Icon = t.Icon,
                Color = t.Color,
                TourCount = usageCounts.ContainsKey(t.Id) ? usageCounts[t.Id] : 0
            }).ToList();
        }

        public async Task<Dictionary<int, string>> GetTagDictionaryAsync()
        {
            return await _tagRepository.GetTagDictionaryAsync();
        }

        public async Task<Dictionary<int, int>> GetTagUsageCountsAsync()
        {
            return await _tagRepository.GetTagUsageCountsAsync();
        }

        public async Task<TagDetailResponse> CreateTagAsync(CreateTagRequest request)
        {
            var slug = GenerateSlug(request.Name);
            var slugExists = await _tagRepository.IsSlugExistsAsync(slug);
            if (slugExists)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            }

            var tag = new Domain.Entities.Tag
            {
                Name = request.Name,
                Slug = slug,
                Icon = request.Icon,
                Color = request.Color
            };

            var created = await _tagRepository.AddAsync(tag);
            return MapToDetailResponse(created, 0);
        }

        public async Task<TagDetailResponse> UpdateTagAsync(int id, UpdateTagRequest request)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with ID {id} not found");

            if (tag.Name != request.Name)
            {
                var newSlug = GenerateSlug(request.Name);
                var slugExists = await _tagRepository.IsSlugExistsAsync(newSlug, id);
                if (slugExists)
                {
                    newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
                }
                tag.Slug = newSlug;
            }

            tag.Name = request.Name;
            tag.Icon = request.Icon;
            tag.Color = request.Color;

            await _tagRepository.UpdateAsync(tag);

            var tourCount = await _tagRepository.GetTourCountByTagAsync(id);
            return MapToDetailResponse(tag, tourCount);
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null) return false;

            await _tagRepository.DeleteAsync(tag);
            return true;
        }

        public async Task<List<TagResponse>> GetOrCreateTagsAsync(List<string> tagNames)
        {
            var tags = await _tagRepository.GetOrCreateTagsAsync(tagNames);
            return tags.Select(MapToResponse).ToList();
        }

        public string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("đ", "d");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            return slug.Trim('-');
        }

        private TagResponse MapToResponse(Domain.Entities.Tag tag)
        {
            return new TagResponse
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                Icon = tag.Icon,
                Color = tag.Color,
                TourCount = tag.TourTags?.Count ?? 0
            };
        }

        private TagDetailResponse MapToDetailResponse(Domain.Entities.Tag tag, int tourCount)
        {
            return new TagDetailResponse
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                Icon = tag.Icon,
                Color = tag.Color,
                TourCount = tourCount,
                Tours = tag.TourTags?.Select(tt => new TourListResponse
                {
                    Id = tt.Tour.Id,
                    Name = tt.Tour.Name,
                    Slug = tt.Tour.Slug,
                    Location = tt.Tour.Location,
                    Price = tt.Tour.Price,
                    Duration = tt.Tour.Duration,
                    AverageRating = tt.Tour.AverageRating
                }).ToList()
            };
        }
    }
}
