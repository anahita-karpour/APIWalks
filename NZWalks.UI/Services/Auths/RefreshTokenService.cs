using NZWalks.UI.Models.DTO;
using NZWalks.UI.Services.Common;

namespace NZWalks.UI.Services.Auths
{
    /// <summary>
    /// Service to manage refresh tokens for the UI
    /// Responsible for: Retrieving, storing, and deleting refresh tokens
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieService _cookieService; //Inject CookieService
        private readonly ILogger<RefreshTokenService> _logger;
        private const string RefreshTokenKey = "refreshToken";

        public RefreshTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            ICookieService cookieService, ILogger<RefreshTokenService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NZWalksAPI"); // Use the named client
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cookieService = cookieService;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
        }
        /// <summary>
        /// Refreshes the JWT token using the refresh token stored in cookies.
        /// </summary>
        /// <returns> string </returns>
        public async Task<string> RefreshTokenAsync()
        {
            try
            {
                string refreshToken = _cookieService.GetCookie("refreshToken");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token not found in cookies.");
                    return null; // Or throw an exception indicating no refresh token
                }

                var response = await _httpClient.PostAsJsonAsync("auth/refresh", new RefreshTokenRequestDto { Token = refreshToken });

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Refresh token request failed:{StatusCode} - {ResponseBody}", response.StatusCode, errorBody);
                    return null; // Or throw an exception indicating the request failed
                }
                var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    _logger.LogError("API refresh token response is invalid.");
                    return null;
                }
                // Store the new JWT token in the cookie
                _cookieService.SetCookie("jwtToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Example expiration for JWT
                });

                // Optionally update the refresh token if the API returns a new one
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    await StoreRefreshToken(result.RefreshToken);
                }

                return result.Token;

            }
            catch (Exception ex)
            {
                //Log the error.
                _logger.LogError(ex, "Error refreshing token.");
                return null;
            }
        }

        /// <summary>
        /// Stores the refresh token in cookies.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task StoreRefreshToken(string refreshToken)
        {
            _cookieService.SetCookie("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7) //Example expiration.
            });
        }

        //public string GetRefreshToken()
        //{
        //    string refreshToken = _cookieService.GetCookie(RefreshTokenKey);
        //    return refreshToken;            
        //}

        /// <summary>
        /// Gets the refresh token from cookies.
        /// </summary>
        /// <returns></returns>
        public string GetRefreshToken() =>
    _cookieService.GetCookie(RefreshTokenKey);

        //It tries to get the token from the session first, then from cookies via _jwtTokenServiceUI.GetJwtTokenFromCookies().
        //private string RetrieveAccessToken()
        //{
        //    var sessionToken = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        //    if (!string.IsNullOrWhiteSpace(sessionToken))
        //    {
        //        return sessionToken;
        //    }

        //    var cookieToken = _jwtTokenServiceUI.GetJwtTokenFromCookies();
        //    if (!string.IsNullOrEmpty(cookieToken))
        //    {
        //        return cookieToken;
        //    }

        //    _logger.LogWarning("No access token found in session or cookies.");
        //    return null;
        //}



        /// <summary>
        /// Deletes the refresh token from cookies.
        /// </summary>
        /// <returns></returns>
        public Task DeleteRefreshToken()
        {
            _cookieService.DeleteCookie(RefreshTokenKey);
            return Task.CompletedTask;
        }


    }
}