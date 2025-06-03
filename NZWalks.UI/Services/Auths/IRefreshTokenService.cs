namespace NZWalks.UI.Services.Auths
{
    public interface IRefreshTokenService
    {
        // This interface clearly defines the contract for any service that provides refresh token management
        Task StoreRefreshToken(string refreshToken);
        string GetRefreshToken();
        Task DeleteRefreshToken();
        Task<string> RefreshTokenAsync();
    }
}
