using Application.Request.User;
using Application.Response.Loyalty;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ILoyaltyService
    {
        Task<LoyaltyInfoResponse> GetLoyaltyInfoAsync(int userId);
        Task<PointsHistoryListResponse> GetPointsHistoryAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> AddPointsAsync(int userId, decimal amountPaid, string description);
        Task RedeemPointsAsync(int userId, int points);
        Task<decimal> ConvertPointsToMoneyAsync(int userId, int pointsToRedeem);
        decimal CalculateDiscount(decimal amount, MemberTier tier);
        int CalculatePointsEarned(decimal amountPaid);
        int CalculateMaxRedeemablePoints(decimal bookingAmount);
        List<MemberTierInfo> GetMemberTierInfo();

     
        /// <summary>
        /// </summary>
        Task<AdminLoyaltyOverviewResponse> GetAdminLoyaltyOverviewAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? tierFilter = null);

        /// <summary>
        /// </summary>
        Task<AdminUserLoyaltyDetailResponse> GetAdminUserLoyaltyDetailAsync(int userId);

        /// <summary>
        /// </summary>
        Task<AdminAllPointsHistoryResponse> GetAdminAllPointsHistoryAsync(
            int page,
            int pageSize,
            string? transactionType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

      
        /// <summary>
        /// </summary>
        Task AdminAdjustPointsAsync(int userId, int points, string reason, string adminEmail);
    }
}