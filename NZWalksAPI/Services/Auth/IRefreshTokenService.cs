using NZWalks.API.Repositories;

namespace NZWalks.API.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshToken(string userId, string jwtId);
        Task<RefreshToken> GetRefreshToken(string token);
        Task<bool> UpdateRefreshToken(RefreshToken refreshToken);
        Task<bool> InvalidateRefreshToken(string token);
    }
}
