namespace NZWalks.UI.Services.Auths
{
    public interface IJwtTokenServiceUI
    {
        string GetAccessTokenFromCookies();
        //Task<string> GetTokenAsync();
        bool IsTokenExpired(string token);
        void SetAccessTokenInCookies(string token);
        Task ClearTokensAsync(); // To clear all authentication related tokens
    }
}
