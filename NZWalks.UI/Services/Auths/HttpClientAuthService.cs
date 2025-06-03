using System.Net.Http.Headers;

namespace NZWalks.UI.Services.Auths
{
    /// <summary>
    /// Service to manage HTTP client authentication for the UI
    /// Responsible for: obtaining the current valid access token, attaching the access token as a Bearer token to outgoing HTTP requests,
    /// Creating an authorized HttpClient, sending authorized requests, 
    /// and refreshing tokens
    /// </summary>
    public class HttpClientAuthService : IHttpClientAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HttpClientAuthService> _logger;
        private readonly IRefreshTokenService _refreshTokenService;

        public HttpClientAuthService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IJwtTokenServiceUI jwtTokenServiceUI,
            IConfiguration configuration, ILogger<HttpClientAuthService> logger,
            IRefreshTokenService refreshTokenService
            )
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jwtTokenServiceUI = jwtTokenServiceUI;
            _configuration = configuration;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
        }

        /// <summary>
        /// Gets an authorized HttpClient with the JWT token from the cookies
        /// </summary>
        /// <returns></returns>
        public async Task<HttpClient> GetAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("NZWalksAPI");
            var token = _jwtTokenServiceUI.GetJwtTokenFromCookies();

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Access token not found in cookies.");
                return client;
            }

            // Use the IsTokenExpired method from JwtTokenServiceUI
            if (_jwtTokenServiceUI.IsTokenExpired(token))
            {
                _logger.LogInformation("Access token expired. Attempting to refresh using RefreshTokenService...");

                var newToken = await _refreshTokenService.RefreshTokenAsync();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    token = newToken;
                    // Update the cookie with the new token
                    _jwtTokenServiceUI.SetJwtTokenInCookies(token);
                }
                else
                {
                    _logger.LogWarning("Refresh token expired or refresh failed.");
                    // optionally clear authentication cookies/session and redirect to login or handle logout
                    return client;
                }
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }


        /// <summary>
        /// Sends an authorized HTTP request.
        /// </summary>
        /// <param name="method">The HTTP method (e.g., GET, POST).</param>
        /// <param name="requestUri">The relative URI for the API endpoint.</param>
        /// <param name="content">The HTTP request content (optional).</param>
        /// <returns>The HTTP response.</returns>
        public async Task<HttpResponseMessage> SendAuthorizedRequest(HttpMethod method, string url, HttpContent content = null)
        {
            var client = await GetAuthorizedClient();

            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            // Set Authorization header explicitly
            var token = _jwtTokenServiceUI.GetJwtTokenFromCookies();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("JWT token is missing. Request may be unauthorized.");
            }

            _logger.LogInformation("Sending authorized request to: {RequestUri}", request.RequestUri);

            return await client.SendAsync(request);
        }
                
    }
}
