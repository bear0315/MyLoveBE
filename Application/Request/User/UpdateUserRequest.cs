using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.User
{
    public class UpdateUserRequest
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string? Avatar { get; set; }
    }
}
