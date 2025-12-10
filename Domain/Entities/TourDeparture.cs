using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TourDeparture : BaseEntity
    {
        public int TourId { get; set; }

        /// <summary>
        /// Ngày khởi hành
        /// </summary>
        public DateTime DepartureDate { get; set; }

        /// <summary>
        /// Ngày kết thúc tour (tự động tính từ DepartureDate + Tour.DurationDays)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho ngày khởi hành này
        /// Mặc định lấy từ Tour.MaxGuests, nhưng có thể điều chỉnh riêng
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// Số lượng khách đã đặt (computed field)
        /// </summary>
        public int BookedGuests { get; set; }

        /// <summary>
        /// Số slot còn lại = MaxGuests - BookedGuests
        /// </summary>
        public int AvailableSlots => MaxGuests - BookedGuests;

        /// <summary>
        /// Giá đặc biệt cho ngày khởi hành này (optional)
        /// Null = dùng giá mặc định từ Tour.Price
        /// </summary>
        public decimal? SpecialPrice { get; set; }

        /// <summary>
        /// Trạng thái ngày khởi hành
        /// </summary>
        public DepartureStatus Status { get; set; } = DepartureStatus.Available;

        /// <summary>
        /// Ghi chú đặc biệt cho ngày khởi hành này
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Guide mặc định cho ngày khởi hành này (optional)
        /// Null = chọn guide theo logic hiện tại
        /// </summary>
        public int? DefaultGuideId { get; set; }

        // Navigation properties
        public Tour Tour { get; set; } = null!;
        public Guide? DefaultGuide { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
