using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Guid
{
    public class UpdateGuideProfileRequest
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(2000)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? Languages { get; set; }

        [MaxLength(500)]
        public string? Experience { get; set; }

        [Range(0, 50)]
        public int? ExperienceYears { get; set; }

        [MaxLength(1000)]
        public string? Specialties { get; set; }

        public string? Avatar { get; set; }
    }
}
