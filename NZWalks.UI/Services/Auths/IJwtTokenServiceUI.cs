namespace NZWalks.UI.Services.Auths
{
    public interface IJwtTokenServiceUI
    {
        string GetJwtTokenFromCookies();
        //Task<string> GetTokenAsync();
        bool IsTokenExpired(string token);
        void SetJwtTokenInCookies(string token);
    }
}
