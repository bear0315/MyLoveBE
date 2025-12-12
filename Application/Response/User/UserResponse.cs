using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; }
        public int YearsOfService { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }


    }
}
