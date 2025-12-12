using Application.Interfaces;
using Application.Request.User;
using Application.Response.Loyalty;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private const int POINTS_PER_10K_VND = 1000; // 10,000 VND = 1000 points
        private const decimal MAX_REDEEM_PERCENTAGE = 0.5m; // Max 50% of booking

        private readonly IUserRepository _userRepository;
        private readonly IPointsHistoryRepository _pointsHistoryRepository;

        public LoyaltyService(
            IUserRepository userRepository,
            IPointsHistoryRepository pointsHistoryRepository)
        {
            _userRepository = userRepository;
            _pointsHistoryRepository = pointsHistoryRepository;
        }

        public async Task<LoyaltyInfoResponse> GetLoyaltyInfoAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var (nextTier, pointsToNext) = GetNextTierInfo(user.LoyaltyPoints, user.MemberTier);

            return new LoyaltyInfoResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                CurrentPoints = user.LoyaltyPoints,
                CurrentTier = user.MemberTier,
                CurrentTierName = GetTierDisplayName(user.MemberTier),
                DiscountPercentage = GetDiscountRate(user.MemberTier),
                NextTier = nextTier,
                NextTierName = nextTier.HasValue ? GetTierDisplayName(nextTier.Value) : null,
                PointsToNextTier = pointsToNext,
                MemberSince = user.MemberSince,
                LastTierUpdateAt = user.LastTierUpdateAt
            };
        }

        public async Task<PointsHistoryListResponse> GetPointsHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            var history = await _pointsHistoryRepository.GetByUserIdAsync(userId, page, pageSize);

            return new PointsHistoryListResponse
            {
                Data = history.Select(h => new PointsHistoryResponse
                {
                    Id = h.Id,
                    TransactionType = h.TransactionType,
                    Points = h.Points,
                    Description = h.Description,
                    BookingCode = h.BookingCode,
                    CreatedAt = h.CreatedAt
                }).ToList(),
                TotalCount = await _pointsHistoryRepository.GetCountByUserIdAsync(userId),
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> AddPointsAsync(int userId, decimal amountPaid, string description)
        {
            var points = CalculatePointsEarned(amountPaid);
            if (points <= 0) return 0;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            user.LoyaltyPoints += points;

            // Check tier upgrade
            var newTier = CalculateTier(user.LoyaltyPoints);
            if (newTier != user.MemberTier)
            {
                user.MemberTier = newTier;
                user.LastTierUpdateAt = DateTime.UtcNow;
            }

            await _userRepository.UpdateAsync(user);

            // Log history
            await _pointsHistoryRepository.CreateAsync(new PointsHistory
            {
                UserId = userId,
                TransactionType = "Earned",
                Points = points,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });

            return points;
        }

        public async Task RedeemPointsAsync(int userId, int points)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            if (user.LoyaltyPoints < points)
                throw new InvalidOperationException("Insufficient points");

            user.LoyaltyPoints -= points;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<decimal> ConvertPointsToMoneyAsync(int userId, int pointsToRedeem)
        {
            if (pointsToRedeem <= 0)
                throw new ArgumentException("Points to redeem must be greater than 0");

            if (pointsToRedeem % 100 != 0)
                throw new ArgumentException("Số điểm phải là bội số của 100");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.LoyaltyPoints < pointsToRedeem)
                throw new InvalidOperationException(
                    $"Không đủ điểm. Bạn có {user.LoyaltyPoints} điểm nhưng cần {pointsToRedeem} điểm");

            // 100 points = 1,000 VND
            decimal moneyValue = (decimal)pointsToRedeem * 10;

            await RedeemPointsAsync(userId, pointsToRedeem);

            return moneyValue;
        }

        public decimal CalculateDiscount(decimal amount, MemberTier tier)
        {
            decimal discountPercentage = GetDiscountRate(tier);
            return Math.Round(amount * discountPercentage, 0);
        }

        public int CalculatePointsEarned(decimal amountPaid)
        {
            // Mỗi 10,000 VND = 100 points (1% cashback)
            return (int)(amountPaid / 10000 * 100);
        }

        public int CalculateMaxRedeemablePoints(decimal bookingAmount)
        {
            decimal maxRedeemValue = bookingAmount * MAX_REDEEM_PERCENTAGE;
            return (int)((maxRedeemValue / 10000) * POINTS_PER_10K_VND);
        }

        public List<MemberTierInfo> GetMemberTierInfo()
        {
            return new List<MemberTierInfo>
            {
                new MemberTierInfo
                {
                    TierName = "Bronze",
                    TierLevel = 0,
                    MinPoints = 0,
                    DiscountPercentage = 0.03m,
                    Benefits = new List<string>
                    {
                        "Giảm giá 3% cho mọi booking",
                        "Tích điểm thưởng 1% giá trị tour",
                        "Đổi điểm thành tiền giảm giá"
                    }
                },
                new MemberTierInfo
                {
                    TierName = "Silver",
                    TierLevel = 1,
                    MinPoints = 10000,
                    DiscountPercentage = 0.05m,
                    Benefits = new List<string>
                    {
                        "Giảm giá 5% cho mọi booking",
                        "Tích điểm thưởng 1% giá trị tour",
                        "Đổi điểm thành tiền giảm giá",
                        "Ưu tiên chọn hướng dẫn viên"
                    }
                },
                new MemberTierInfo
                {
                    TierName = "Gold",
                    TierLevel = 2,
                    MinPoints = 50000,
                    DiscountPercentage = 0.10m,
                    Benefits = new List<string>
                    {
                        "Giảm giá 10% cho mọi booking",
                        "Tích điểm thưởng 1% giá trị tour",
                        "Đổi điểm thành tiền giảm giá",
                        "Ưu tiên chọn hướng dẫn viên",
                        "Hỗ trợ khách hàng ưu tiên",
                        "Quà tặng sinh nhật"
                    }
                }
            };
        }

        // Private helpers
        private decimal GetDiscountRate(MemberTier tier) => tier switch
        {
            MemberTier.Bronze => 0.03m,
            MemberTier.Silver => 0.05m,
            MemberTier.Gold => 0.10m,
            _ => 0m
        };

        private string GetTierDisplayName(MemberTier tier) => tier switch
        {
            MemberTier.Bronze => "Đồng",
            MemberTier.Silver => "Bạc",
            MemberTier.Gold => "Vàng",
            _ => "Unknown"
        };

        private MemberTier CalculateTier(int points)
        {
            if (points >= 50000) return MemberTier.Gold;
            if (points >= 10000) return MemberTier.Silver;
            return MemberTier.Bronze;
        }

        private (MemberTier? nextTier, int? pointsToNext) GetNextTierInfo(int currentPoints, MemberTier currentTier)
        {
            return currentTier switch
            {
                MemberTier.Bronze => (MemberTier.Silver, 10000 - currentPoints),
                MemberTier.Silver => (MemberTier.Gold, 50000 - currentPoints),
                MemberTier.Gold => (null, null),
                _ => (null, null)
            };
        }
    }
}

