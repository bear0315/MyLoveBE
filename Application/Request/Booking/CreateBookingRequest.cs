using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Request.Booking
{
    public class CreateBookingRequest
    {
        [Required]
        public int TourId { get; set; }

        public int? TourDepartureId { get; set; }

        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Number of guests must be between 1 and 100")]
        public int NumberOfGuests { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Points to redeem must be non-negative")]
        public int? PointsToRedeem { get; set; }

        public int? GuideId { get; set; }

        // ============ PRICING INFO (NEW) ============
        /// <summary>
        /// Giá gốc = (Price per person * Guests) + Service Fee
        /// Calculated by frontend before applying any discounts
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Tổng tiền cuối cùng sau khi trừ tất cả giảm giá
        /// = OriginalAmount - MemberDiscount - PointsDiscount
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Giảm giá từ hạng thành viên (VND)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal MemberDiscount { get; set; }

        /// <summary>
        /// Tên hạng thành viên (Bronze, Silver, Gold, Platinum)
        /// </summary>
        [MaxLength(50)]
        public string? MemberTier { get; set; }

        /// <summary>
        /// Phần trăm giảm giá thành viên (0.05 = 5%)
        /// </summary>
        [Range(0, 1)]
        public decimal MemberDiscountPercentage { get; set; }

        /// <summary>
        /// Giảm giá từ đổi điểm (VND)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PointsDiscount { get; set; }

        /// <summary>
        /// Số điểm tích lũy sau khi hoàn thành tour
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PointsToEarn { get; set; }
        // ============================================

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public List<BookingGuestRequest> Guests { get; set; } = new();
    }
}