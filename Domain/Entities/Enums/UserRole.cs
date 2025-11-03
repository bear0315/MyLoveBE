using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum UserRole
    {
        Customer = 0,
        Guide = 1,      // Hướng dẫn viên
        Staff = 2,      // Nhân viên
        Manager = 3,    // Quản lý
        Admin = 4       // Admin
    }
}
