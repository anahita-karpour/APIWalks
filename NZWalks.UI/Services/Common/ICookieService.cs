namespace NZWalks.UI.Services.Common
{
    public interface ICookieService
    {
        //This interface clearly defines the contract for any service that provides cookie management
        string GetCookie(string key);
        void SetCookie(string key, string value, CookieOptions options);
        void DeleteCookie(string key);
    }
}
