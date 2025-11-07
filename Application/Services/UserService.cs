using Application.Interfaces;
using Application.Request.User;
using Application.Response.User;
using Application.Utilities;
using Application.Mappings;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public UserService(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<BaseResponse<UserResponse>> GetByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new BaseResponse<UserResponse>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user.ToUserResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<UserResponse>> GetByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new BaseResponse<UserResponse>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user.ToUserResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        public async Task<UserListResponse> GetAllAsync(int page = 1, int pageSize = 10, string? role = null, bool? isActive = null)
        {
            try
            {
                var query = _userRepository.GetAll();

                // Filter by role
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by active status
                if (isActive.HasValue)
                {
                    var status = isActive.Value ? UserStatus.Active : UserStatus.Inactive;
                    query = query.Where(u => u.Status == status);
                }

                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new UserListResponse
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users.ToUserResponseList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new UserListResponse
                {
                    Success = false,
                    Message = $"Error retrieving users: {ex.Message}",
                    Data = new List<UserResponse>()
                };
            }
        }

        public async Task<BaseResponse<UserResponse>> CreateAsync(CreateUserRequest request)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                // Validate password
                if (!PasswordHasher.IsPasswordValid(request.Password, out string passwordError))
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = passwordError
                    };
                }

                // Parse role
                if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Invalid role"
                    };
                }

                // Create user entity
                var user = new User
                {
                    Email = request.Email.ToLower().Trim(),
                    PasswordHash = PasswordHasher.HashPassword(request.Password),
                    FullName = request.FullName.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    Avatar = request.Avatar,
                    Role = userRole,
                    Status = UserStatus.Active,
                    MemberSince = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Log audit
                await LogAuditAsync(createdUser.Id, "USER_CREATED", "User", createdUser.Id,
                    $"User created: {createdUser.Email}");

                return new BaseResponse<UserResponse>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = createdUser.ToUserResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<UserResponse>> UpdateAsync(int id, UpdateUserRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FullName))
                {
                    user.FullName = request.FullName.Trim();
                }

                if (request.PhoneNumber != null)
                {
                    user.PhoneNumber = request.PhoneNumber.Trim();
                }

                if (request.Avatar != null)
                {
                    user.Avatar = request.Avatar;
                }

                var updatedUser = await _userRepository.UpdateAsync(user);

                // Log audit
                await LogAuditAsync(id, "USER_UPDATED", "User", id,
                    $"User updated: {updatedUser.Email}");

                return new BaseResponse<UserResponse>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = updatedUser.ToUserResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error updating user: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> ChangePasswordAsync(int id, ChangePasswordRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Verify current password
                if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Validate new password
                if (!PasswordHasher.IsPasswordValid(request.NewPassword, out string passwordError))
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = passwordError
                    };
                }

                // Update password
                user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
                await _userRepository.UpdateAsync(user);

                // Log audit
                await LogAuditAsync(id, "PASSWORD_CHANGED", "User", id,
                    $"Password changed for user: {user.Email}");

                return new BaseResponse
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Error changing password: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<UserResponse>> UpdateStatusAsync(int id, UpdateUserStatusRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update status
                user.Status = request.IsActive ? UserStatus.Active : UserStatus.Inactive;
                var updatedUser = await _userRepository.UpdateAsync(user);

                // Log audit
                await LogAuditAsync(id, "USER_STATUS_UPDATED", "User", id,
                    $"User status changed to {user.Status}: {user.Email}");

                return new BaseResponse<UserResponse>
                {
                    Success = true,
                    Message = "User status updated successfully",
                    Data = updatedUser.ToUserResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error updating user status: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> DeleteAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var result = await _userRepository.DeleteAsync(id);

                if (!result)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Failed to delete user"
                    };
                }

                await LogAuditAsync(id, "USER_DELETED", "User", id,
                    $"User deleted: {user.Email}");

                return new BaseResponse
                {
                    Success = true,
                    Message = "User deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Error deleting user: {ex.Message}"
                };
            }
        }

        private async Task LogAuditAsync(int? userId, string action, string entityName,
            int entityId, string? newValues = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    NewValues = newValues
                };

                await _auditLogRepository.CreateAsync(auditLog);
            }
            catch
            {
                // Swallow audit logging errors
            }
        }
    }
}