using Application.Interfaces;
using Application.Mappings;
using Application.Request.Login;
using Application.Response.Login;
using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IAuditLogRepository auditLogRepository,
            IJwtService jwtService, 
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _auditLogRepository = auditLogRepository;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    await LogAuditAsync(null, "LOGIN_FAILED", "User", 0,
                        $"Login attempt with email: {request.Email}", request.IpAddress);
                    return AuthMapper.ToFailedLoginResponse("Invalid email or password");
                }

                //if (user.Status != UserStatus.Active)
                //{
                //    await LogAuditAsync(user.Id, "LOGIN_BLOCKED", "User", user.Id,
                //        $"Login blocked - Status: {user.Status}", request.IpAddress);
                //    return AuthMapper.ToFailedLoginResponse("Account is inactive or blocked");
                //}

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    await LogAuditAsync(user.Id, "LOGIN_FAILED", "User", user.Id,
                        "Invalid password attempt", request.IpAddress);
                    return AuthMapper.ToFailedLoginResponse("Invalid email or password");
                }

                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60"));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                    int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7"));

                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    DeviceInfo = request.DeviceInfo,
                    IpAddress = request.IpAddress
                };
                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                await LogAuditAsync(user.Id, "LOGIN_SUCCESS", "User", user.Id,
                    $"Successful login from device: {request.DeviceInfo}", request.IpAddress);

                return user.ToLoginResponse(
                    accessToken,
                    refreshToken,
                    accessTokenExpiry,
                    refreshTokenExpiry);
            }
            catch (Exception ex)
            {
                await LogAuditAsync(null, "LOGIN_ERROR", "User", 0,
                    $"Login error: {ex.Message}", request.IpAddress);
                return AuthMapper.ToFailedLoginResponse("An error occurred during login");
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
                if (refreshTokenEntity == null)
                {
                    return AuthMapper.ToFailedLoginResponse("Invalid refresh token");
                }

                if (!refreshTokenEntity.IsActive)
                {
                    await LogAuditAsync(refreshTokenEntity.UserId, "TOKEN_REFRESH_FAILED",
                        "RefreshToken", refreshTokenEntity.Id,
                        "Attempted to use expired or revoked token", request.IpAddress);
                    return AuthMapper.ToFailedLoginResponse("Refresh token is expired or revoked");
                }

                var user = refreshTokenEntity.User;
                if (user.Status != UserStatus.Active)
                {
                    return AuthMapper.ToFailedLoginResponse("Account is inactive or blocked");
                }

                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60"));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                    int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7"));

                refreshTokenEntity.IsRevoked = true;
                refreshTokenEntity.RevokedAt = DateTime.UtcNow;
                refreshTokenEntity.ReplacedByToken = newRefreshToken;
                await _refreshTokenRepository.UpdateAsync(refreshTokenEntity);

                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    DeviceInfo = request.DeviceInfo,
                    IpAddress = request.IpAddress
                };
                await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

                await LogAuditAsync(user.Id, "TOKEN_REFRESHED", "RefreshToken",
                    newRefreshTokenEntity.Id, "Token successfully refreshed", request.IpAddress);

                return user.ToLoginResponse(
                    newAccessToken,
                    newRefreshToken,
                    accessTokenExpiry,
                    refreshTokenExpiry,
                    "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                await LogAuditAsync(null, "TOKEN_REFRESH_ERROR", "RefreshToken", 0,
                    $"Token refresh error: {ex.Message}", request.IpAddress);
                return AuthMapper.ToFailedLoginResponse("An error occurred during token refresh");
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var result = await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);

                if (result)
                {
                    await LogAuditAsync(userId, "LOGOUT", "User", userId, "User logged out");
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogAuditAsync(userId, "LOGOUT_ERROR", "User", userId,
                    $"Logout error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (tokenEntity == null)
                    return false;

                var result = await _refreshTokenRepository.RevokeAsync(refreshToken);

                if (result)
                {
                    await LogAuditAsync(tokenEntity.UserId, "TOKEN_REVOKED",
                        "RefreshToken", tokenEntity.Id, "Refresh token manually revoked");
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogAuditAsync(null, "TOKEN_REVOKE_ERROR", "RefreshToken", 0,
                    $"Token revoke error: {ex.Message}");
                return false;
            }
        }

        #region Private Helper Methods
        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private async Task LogAuditAsync(int? userId, string action, string entityName,
            int entityId, string? newValues = null, string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    NewValues = newValues,
                    IpAddress = ipAddress
                };

                await _auditLogRepository.CreateAsync(auditLog);
            }
            catch
            {
            }
        }

        #endregion
    }
}