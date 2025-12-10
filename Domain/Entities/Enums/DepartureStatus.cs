using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum DepartureStatus
    {
        Available = 0,      // Còn chỗ, có thể đặt
        AlmostFull = 1,     // Sắp hết chỗ (< 20% slots)
        Full = 2,           // Hết chỗ
        Cancelled = 3,      // Đã hủy
        Completed = 4       // Đã hoàn thành
    }
}
