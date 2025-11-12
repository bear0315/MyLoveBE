using Application.Response.Guide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGuideService
    {
        Task<List<GuideListResponse>> GetGuidesByTourIdAsync(int tourId);
        Task<GuideListResponse?> GetDefaultGuideForTourAsync(int tourId);
        Task<List<GuideAvailabilityResponse>> GetAvailableGuidesAsync(int tourId, DateTime tourDate);
        Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate);
        Task<bool> ValidateGuideForTourAsync(int tourId, int guideId);
    }
}
