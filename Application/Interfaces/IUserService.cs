using Application.Request.User;
using Application.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserResponse>> GetByIdAsync(int id);
        Task<BaseResponse<UserResponse>> GetByEmailAsync(string email);
        Task<UserListResponse> GetAllAsync(int page = 1, int pageSize = 10, string? role = null, bool? isActive = null);
        Task<BaseResponse<UserResponse>> CreateAsync(CreateUserRequest request);
        Task<BaseResponse<UserResponse>> UpdateAsync(int id, UpdateUserRequest request);
        Task<BaseResponse> ChangePasswordAsync(int id, ChangePasswordRequest request);
        Task<BaseResponse<UserResponse>> UpdateStatusAsync(int id, UpdateUserStatusRequest request);
        Task<BaseResponse> DeleteAsync(int id);
    }
}
