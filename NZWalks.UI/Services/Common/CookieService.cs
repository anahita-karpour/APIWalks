namespace NZWalks.UI.Services.Common // Or NZWalks.UI.Services
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CookieService> _logger;

        public CookieService(IHttpContextAccessor httpContextAccessor, ILogger<CookieService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void SetCookie(string key, string value, CookieOptions options)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogWarning("HttpContext is null while trying to set cookie: {Key}", key);
                return;
            }

            if (string.IsNullOrEmpty(key))
                _logger.LogError("Cookie key is null or empty.");
            if (string.IsNullOrEmpty(value))
                _logger.LogError("Cookie value is null or empty.");

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, options);
            }
        }

        public string GetCookie(string key)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogWarning("HttpContext is null while trying to get cookie: {Key}", key);
                return null;
            }
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Cookie key is null or empty.");
                return null;
            }

            return _httpContextAccessor.HttpContext?.Request.Cookies[key];
        }

        public void DeleteCookie(string key)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogWarning("HttpContext is null while trying to delete cookie: {Key}", key);
                return;
            }
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Cookie key is null or empty.");
                return;
            }

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(key);
        }
    }
}