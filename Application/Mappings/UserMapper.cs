using Application.Response.User;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public static class UserMapper
    {
        public static UserResponse ToUserResponse(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var yearsOfService = (DateTime.UtcNow - user.MemberSince).Days / 365;

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                DateOfBirth = null,
                JoinDate = user.MemberSince,
                YearsOfService = yearsOfService,
                Roles = new List<string> { user.Role.ToString() },
                IsActive = user.Status == Domain.Entities.Enums.UserStatus.Active
            };
        }

        public static List<UserResponse> ToUserResponseList(this IEnumerable<User> users)
        {
            return users.Select(u => u.ToUserResponse()).ToList();
        }
    }
}
