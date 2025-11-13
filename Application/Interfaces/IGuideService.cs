using Application.Request.Guid;
using Application.Response.Guide;
using Application.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGuideService
    {
        Task<List<GuideListResponse>> GetAllGuidesAsync();
        Task<GuideDetailResponse?> GetGuideByIdAsync(int id);
        Task<List<GuideListResponse>> GetActiveGuidesAsync();
        Task<List<GuideListResponse>> SearchGuidesAsync(string? keyword = null, string? language = null);
        Task<BaseResponse<GuideProfileResponse>> UpdateProfileAsync(int userId, UpdateGuideProfileRequest request);
        Task<GuideDetailResponse> GetGuideByUserIdAsync(int userId);


    }
}
