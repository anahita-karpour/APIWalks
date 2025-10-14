using NZWalks.UI.Services.Auths;

namespace NZWalks.UI.Services
{
    public abstract class BaseApiService
    {
        protected readonly ILogger _logger;
        protected readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        protected readonly IRefreshTokenService _refreshTokenService;
        protected readonly IHttpClientAuthService _httpClientAuthService; // Used by the action lambda

        public BaseApiService(
            ILogger logger,
            IJwtTokenServiceUI jwtTokenServiceUI,
            IRefreshTokenService refreshTokenService,
            IHttpClientAuthService httpClientAuthService)
        {
            _logger = logger;
            _jwtTokenServiceUI = jwtTokenServiceUI;
            _refreshTokenService = refreshTokenService;
            _httpClientAuthService = httpClientAuthService;
        }

        /// <summary>
        /// Orchestrates the retry logic. 
        /// It uses IJwtTokenServiceUI to set the new access token after the refresh
        /// and uses IRefreshTokenService to perform the refresh API call.
        /// Its job is to wrap any API call that might fail due to an expired access token, 
        /// then trigger a token refresh, and retry that original API call.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        protected async Task<HttpResponseMessage> ExecuteWithRefreshRetry(Func<Task<HttpResponseMessage>> action)
        {
            // First attempt at the API call
            var response = await action();

            // Check if the first attempt resulted in an Unauthorized status
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized (401) response received. Attempting token refresh.");

                try
                {
                    // *** Step 1: Perform the actual token refresh API call ***
                    // This method in IRefreshTokenService should:
                    // 1. Get the stored refresh token.
                    // 2. Make an HTTP POST request to your Auth Server's /refresh endpoint.
                    // 3. Receive new access and refresh tokens from the Auth Server.
                    // 4. Store the new refresh token (e.g., via _refreshTokenService.StoreRefreshToken()).
                    // 5. Return the new access token.
                    var newAccessToken = await _refreshTokenService.RefreshTokenAsync();

                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        // *** Step 2: Store the newly obtained access token ***
                        // This ensures that subsequent calls or retries use the valid token.
                        _jwtTokenServiceUI.SetAccessTokenInCookies(newAccessToken);

                        _logger.LogInformation("Access token successfully refreshed. Retrying original request.");

                        // *** Step 3: Retry the original API call ***
                        // The 'action' lambda will call `_httpClientAuthService.GetAuthorizedClient()`,
                        // which should now pick up the 'newAccessToken' just stored by SetJwtTokenInCookies.
                        response = await action();
                    }
                    else
                    {
                        // RefreshTokenAsync returned null or empty, indicating refresh failed.
                        _logger.LogError("Token refresh failed: RefreshTokenAsync returned null or empty. Clearing all tokens.");
                        await _jwtTokenServiceUI.ClearTokensAsync(); // Clear access token
                        await _refreshTokenService.DeleteRefreshToken(); // Clear refresh token
                                                                         // Propagate a specific exception for the controller to handle (e.g., redirect to login)
                        throw new UnauthorizedAccessException("Your session has expired. Please log in again.");
                    }
                }
                catch (HttpRequestException refreshEx) // Catch specific HTTP errors during the refresh API call itself
                {
                    _logger.LogError(refreshEx, "HTTP error during token refresh API call. Clearing all tokens.");
                    await _jwtTokenServiceUI.ClearTokensAsync();
                    await _refreshTokenService.DeleteRefreshToken();
                    throw new UnauthorizedAccessException("Network error during session refresh. Please log in again.", refreshEx);
                }
                catch (Exception refreshEx) // Catch any other unexpected errors during the refresh process
                {
                    _logger.LogError(refreshEx, "An unexpected error occurred during token refresh. Clearing all tokens.");
                    await _jwtTokenServiceUI.ClearTokensAsync();
                    await _refreshTokenService.DeleteRefreshToken();
                    throw new UnauthorizedAccessException("An unexpected error occurred during session refresh. Please log in again.", refreshEx);
                }
            }

            // Return the response from either the first attempt (if not 401) or the retried attempt.
            // The calling method (like AddRegionAsync) will then handle EnsureSuccessStatusCode() and deserialization.
            return response;
        }
    }
}
