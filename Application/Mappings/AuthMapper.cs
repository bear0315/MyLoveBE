using Application.Request.User;
using Application.Response.Login;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public static class AuthMapper
    {
        public static UserRequest ToUserRequest(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserRequest
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                LastLoginAt = user.LastLoginAt,
                MemberSince = user.MemberSince
            };
        }

        public static LoginResponse ToLoginResponse(
            this User user,
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiresAt,
            DateTime refreshTokenExpiresAt,
            string message = "Login successful")
        {
            return new LoginResponse
            {
                Success = true,
                Message = message,
                User = user.ToUserRequest(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public static LoginResponse ToFailedLoginResponse(string message)
        {
            return new LoginResponse
            {
                Success = false,
                Message = message,
                User = null,
                AccessToken = null,
                RefreshToken = null
            };
        }

        public static RefreshTokenResponse ToRefreshTokenResponse(
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiresAt,
            DateTime refreshTokenExpiresAt,
            string message = "Token refreshed successfully")
        {
            return new RefreshTokenResponse
            {
                Success = true,
                Message = message,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public static RefreshTokenResponse ToFailedRefreshTokenResponse(string message)
        {
            return new RefreshTokenResponse
            {
                Success = false,
                Message = message,
                AccessToken = null,
                RefreshToken = null
            };
        }
    }
}
