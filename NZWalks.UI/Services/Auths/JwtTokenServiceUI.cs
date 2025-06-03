using System.IdentityModel.Tokens.Jwt;

namespace NZWalks.UI.Services.Auths
{
    /// <summary>
    /// Service to set and get JWT tokens from cookies for the UI
    /// </summary>
    public class JwtTokenServiceUI : IJwtTokenServiceUI
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtTokenServiceUI> _logger;
        private readonly IConfiguration _configuration;

        public JwtTokenServiceUI(IHttpContextAccessor httpContextAccessor, // JwtTokenServiceUI needs access to the HttpContext to interact with cookies.
            ILogger<JwtTokenServiceUI> logger,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get the JWT token from jwtToken cookie 
        /// </summary>
        /// <returns>String</returns>
        public string GetJwtTokenFromCookies()
        {
            try
            {
                if (_httpContextAccessor.HttpContext == null)
                {
                    _logger.LogWarning("HttpContext is null. Cannot retrieve JWT token.");
                    return null;
                }

                // Get the JWT token from the cookies
                //var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwtToken"];
                var jwtCookieName = _configuration["Jwt:JWTCookieName"] ?? "jwtToken";
                var token = _httpContextAccessor.HttpContext.Request.Cookies[jwtCookieName];


                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("JWT Token was null or empty");
                    return null;
                }

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting JWT Token from Cookies");
                return null;
            }
        }

        /// <summary>
        /// Check if the token is expired
        ///</summary>
        /// <param name="token">The JWT token</param>
        /// <returns>Boolean</returns>
        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true; // Or handle this case as appropriate
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return true; // Or throw an exception for an invalid token format
            }

            var jwtToken = handler.ReadJwtToken(token);
            var expiryClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.ToLower() == "exp");

            if (expiryClaim == null)
            {
                return false; // Token doesn't have an expiration claim, assume not expired (handle carefully)
            }

            if (long.TryParse(expiryClaim.Value, out long expiryUnix))
            {
                var expiryDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime;
                return expiryDateTimeUtc <= DateTime.UtcNow;
            }

            return true; // Error parsing expiration, assume expired
        }

        /// <summary>
        /// Set the JWT token in the cookies
        /// It checks if HttpContext is available and sets the JWT token in the cookie in the user's browser
        /// with HttpOnly, Secure, and SameSite attributes.
        /// </summary>
        /// <param name="token"> 
        /// The JWT token to set in the cookies
        /// </param>
        public void SetJwtTokenInCookies(string token)
        {
            if (_httpContextAccessor.HttpContext != null && !string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var expiryClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.ToLower() == "exp");

                    if (expiryClaim != null && long.TryParse(expiryClaim.Value, out long expiryUnix))
                    {
                        var expiryDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).ToLocalTime(); // Or .UtcDateTime

                        _httpContextAccessor.HttpContext.Response.Cookies.Append(
                            _configuration["Jwt:JWTCookieName"] ?? "jwtToken",
                            token,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                                SameSite = SameSiteMode.Strict,
                                Expires = expiryDateTimeOffset // Set cookie expiration from JWT
                            }
                        );
                        return; // Exit if successful
                    }
                    else
                    {
                        _logger.LogWarning("Could not find or parse 'exp' claim in JWT. Using default cookie expiration.");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid JWT format. Using default cookie expiration.");
                }

                // Fallback to a default expiration if JWT parsing fails
                _httpContextAccessor.HttpContext.Response.Cookies.Append(
                    _configuration["Jwt:JWTCookieName"] ?? "jwtToken",
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Default fallback
                    }
                );
            }
        }
    }
}