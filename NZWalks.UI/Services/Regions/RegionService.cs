using AutoMapper;
using NZWalks.API.Models.DTO;
using NZWalks.UI.Services.Auths;

namespace NZWalks.UI.Services.Regions
{
    public class RegionService : BaseApiService, IRegionService
    {
        private readonly IHttpClientAuthService _httpClientAuthService;
        private readonly ILogger<RegionService> _logger;
        private readonly UI.Services.Auths.IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        private readonly IMapper _mapper;
        //private readonly HttpClient _httpClient;

        public RegionService(
            IHttpClientAuthService httpClientAuthService,
            ILogger<RegionService> logger,
            UI.Services.Auths.IRefreshTokenService refreshTokenService,
            IJwtTokenServiceUI jwtTokenServiceUI,
            IMapper mapper
            //, HttpClient httpClient
            ) : base(logger, jwtTokenServiceUI, refreshTokenService, httpClientAuthService) // Call the base constructor to initialize the base class
        {
            _httpClientAuthService = httpClientAuthService;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
            _jwtTokenServiceUI = jwtTokenServiceUI;
            _mapper = mapper;
            //_httpClient = httpClient;
        }

        /// <summary>
        /// Get all regions from the API
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<RegionDtoV1>> GetAllRegionsAsync()
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(async () => // async lambda to allow await inside
                {   // Get a new authorized client for each attempt within the retry logic
                    var client = await _httpClientAuthService.GetAuthorizedClient();
                    return await client.GetAsync("api/v1/regions");
                });

                response.EnsureSuccessStatusCode();

                var regions = await response.Content.ReadFromJsonAsync<IEnumerable<RegionDtoV1>>();
                return regions ?? new List<RegionDtoV1>();
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching all regions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching all regions.");
            }

            return new List<RegionDtoV1>();
        }

        /// <summary>
        /// Http call to the API for getting a region by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RegionDtoV1> GetRegionByIdAsync(Guid id)
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(async () => // Use async lambda
                {
                    // 1- Get a new authorized client for each attempt within the retry logic
                    var client = await _httpClientAuthService.GetAuthorizedClient();
                    // 2- Make the actual GET request
                    return await client.GetAsync($"api/v1/regions/{id}");
                });

                response.EnsureSuccessStatusCode();

                var region = await response.Content.ReadFromJsonAsync<RegionDtoV1>();
                return region;
            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while fetching region with ID {RegionId}.", id);
                return null; // return an empty list of regions or an appropriate error message
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching region with ID {RegionId}.", id);
                return null; // return an empty list instead of null
            }
        }

        /// <summary>
        /// Http call to the API for adding a region
        /// </summary>
        /// <param name="addRegionViewModel"></param>
        /// <returns></returns>
        public async Task<RegionDtoV1> AddRegionAsync(AddRegionRequestDto addRegionRequestDto)
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(async () => // Use async lambda
                {
                    // 1- Get a new authorized client for each attempt within the retry logic
                    var client = await _httpClientAuthService.GetAuthorizedClient();
                    // 2- Make the actual GET request
                    return await client.PostAsJsonAsync($"api/v1/regions", addRegionRequestDto);
                });

                response.EnsureSuccessStatusCode(); // throws if not successful

                return await response.Content.ReadFromJsonAsync<RegionDtoV1>();

            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while adding region.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding region.");
                return null;
            }
        }

        /// <summary>
        /// Handles HTTP call to the API for updating a region
        /// </summary>
        /// <param name="updateRegionViewModel"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRegionAsync(UpdateRegionRequestDto updateRegionRequestDto)
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(async () =>
                {
                    var client = await _httpClientAuthService.GetAuthorizedClient();
                    return await client.PutAsJsonAsync($"api/v1/regions/{updateRegionRequestDto.Id}", updateRegionRequestDto); // send the DTO directly
                });

                response.EnsureSuccessStatusCode();

                // We don't need to read content if the API isn't sending back the object.
                return true;
            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while updating region with Code {RegionCode}.", updateRegionRequestDto.Code);
                return false; // indicate failure
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating region with Code {RegionCode}.", updateRegionRequestDto.Code);
                return false; // indicate failure
            }
        }

        /// <summary>
        /// Handles HTTP call to the API for deleting a region
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRegionAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid region ID provided for deletion.");
                    return false;
                }

                var response = await ExecuteWithRefreshRetry(async () => // Use async lambda here
                {
                    // 1- Get a new authorized client for each attempt within the retry logic
                    var client = await _httpClientAuthService.GetAuthorizedClient();
                    // 2- Make the actual DELETE request
                    return await client.DeleteAsync($"api/v1/regions/{id}");
                });

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Region with ID {RegionId} deleted successfully.", id);
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while deleting region with ID {RegionId}.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting region with ID {RegionId}.", id);
            }
            return false;
        }

        //private async Task<HttpResponseMessage> ExecuteWithRefreshRetry(Func<Task<HttpResponseMessage>> action)
        //{
        //    var response = await action();

        //    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //    {
        //        _logger.LogWarning("Unauthorized (401). Attempting token refresh.");

        //        var newAccessToken = _refreshTokenService.GetRefreshToken();
        //        if (!string.IsNullOrEmpty(newAccessToken))
        //        {
        //            _logger.LogInformation("Token refreshed. Retrying request.");
        //            return await action(); // Retry the original action
        //        }

        //        _logger.LogError("Token refresh failed. Clearing refresh token.");
        //        await _refreshTokenService.DeleteRefreshToken();
        //    }

        //    return response;
        //}

        //    /// <summary>
        //    /// Orchestrates the retry logic. 
        //    /// It uses IJwtTokenServiceUI to set the new access token after the refresh
        //    /// and uses IRefreshTokenService to perform the refresh API call.
        //    /// Its job is to wrap any API call that might fail due to an expired access token, 
        //    /// then trigger a token refresh, and retry that original API call.
        //    /// </summary>
        //    /// <param name="action"></param>
        //    /// <returns></returns>
        //    /// <exception cref="UnauthorizedAccessException"></exception>
        //    private async Task<HttpResponseMessage> ExecuteWithRefreshRetry(Func<Task<HttpResponseMessage>> action)
        //    {
        //        // First attempt
        //        var response = await action();

        //        // Check if the first attempt was unauthorized
        //        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //        {
        //            _logger.LogWarning("API call returned 401 Unauthorized. Attempting token refresh.");

        //            // Call the service that handles the actual token refresh API call and token storage
        //            var refreshTokenResult = await _jwtTokenServiceUI.RefreshAccessTokenAsync(); // Assumes this method does the actual API call and stores new tokens

        //            if (refreshTokenResult.IsSuccess) // Assumes RefreshAccessTokenAsync returns a result object (e.g., containing IsSuccess)
        //            {
        //                _logger.LogInformation("Token refreshed. Retrying request.");
        //                // The `action()` will now implicitly get a client with the new token
        //                // because `_httpClientAuthService.GetAuthorizedClient()` (used inside your `action` lambda)
        //                // should retrieve the *recently stored* new access token.
        //                response = await action(); // Retry the original API call
        //            }
        //            else
        //            {
        //                // Token refresh failed (e.g., refresh token expired, network error during refresh)
        //                _logger.LogError("Failed to refresh token: {Error}. Clearing tokens.", refreshTokenResult.ErrorMessage);
        //                await _jwtTokenServiceUI.ClearTokensAsync(); // Clear all tokens if refresh fails
        //                                                             // Propagate an exception indicating a full session expiration for the controller to handle
        //                throw new UnauthorizedAccessException("Your session has expired. Please log in again.");
        //            }
        //        }

        //        // This method only handles the 401 and retry.
        //        // The calling method (e.g., AddRegionAsync) will still need to handle EnsureSuccessStatusCode()
        //        // and deserialization, as per your current setup.
        //        return response;
        //    }
    }
}
