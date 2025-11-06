using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetActiveTokenByUserIdAsync(int userId, string deviceInfo);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
        Task<bool> RevokeAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(int userId);
        Task<bool> DeleteExpiredTokensAsync();
    }
}
