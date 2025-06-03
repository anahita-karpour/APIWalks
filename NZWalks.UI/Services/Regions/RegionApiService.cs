using AutoMapper;
using NZWalks.UI.Models.DTO;
using NZWalks.UI.Models.ViewModel;
using NZWalks.UI.Services.Auths;

namespace NZWalks.UI.Services.Regions
{
    public class RegionApiService
    {
        private readonly IHttpClientAuthService _httpClientAuthService;
        private readonly ILogger<RegionApiService> _logger;
        private readonly UI.Services.Auths.IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;

        public RegionApiService(
            IHttpClientAuthService httpClientAuthService,
            ILogger<RegionApiService> logger,
            UI.Services.Auths.IRefreshTokenService refreshTokenService,
            IJwtTokenServiceUI jwtTokenServiceUI,
            IMapper mapper,
            HttpClient httpClient
            )
        {
            _httpClientAuthService = httpClientAuthService;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
            _jwtTokenServiceUI = jwtTokenServiceUI;
            _mapper = mapper;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get all regions from the API
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<RegionDto>> GetAllRegionsAsync()
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(() =>
                    _httpClientAuthService.SendAuthorizedRequest(HttpMethod.Get, "api/v1/regions"));

                response.EnsureSuccessStatusCode();


                //if (response.IsSuccessStatusCode)
                //{
                var regions = await response.Content.ReadFromJsonAsync<IEnumerable<RegionDto>>();
                return regions ?? new List<RegionDto>();
                //}
                //else
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Access forbidden (403). Insufficient permissions.");
                }
                else
                {
                    _logger.LogError("Error fetching regions: {StatusCode} - {Content}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                }
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching all regions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching all regions.");
            }

            return new List<RegionDto>();
        }

        /// <summary>
        /// Http call to the API for getting a region by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RegionDto> GetRegionByIdAsync(Guid id)
        {
            try
            {
                // 1. get the authorized client                
                var client = await _httpClientAuthService.GetAuthorizedClient();

                // 2. make the request
                //var response = await client.GetFromJsonAsync<RegionDto>($"api/v1/regions/{id}");
                //var response = await client.GetAsync($"api/v1/regions/{id}");
                var response = await ExecuteWithRefreshRetry(() => client.GetAsync($"api/v1/regions/{id}"));

                response.EnsureSuccessStatusCode();

                var region = await response.Content.ReadFromJsonAsync<RegionDto>();
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
        public async Task<RegionDto> AddRegionAsync(AddRegionViewModel addRegionViewModel)
        {
            try
            {
                // 1. get the authorized client
                var client = await _httpClientAuthService.GetAuthorizedClient();
                // 2. make the request
                var response = await client.PostAsJsonAsync("api/v1/regions", addRegionViewModel);

                response.EnsureSuccessStatusCode(); // throws if not successful

                return await response.Content.ReadFromJsonAsync<RegionDto>();

                //if (response.IsSuccessStatusCode)
                //{
                //    var createdRegion = await response.Content.ReadFromJsonAsync<RegionDto>();
                //    return createdRegion;
                //}
                //else
                //{
                //    _logger.LogError("Error adding region: {StatusCode}", response.StatusCode);
                //    return null;
                //}
            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while adding region.");
                return null; // return an empty list of regions or an appropriate error message
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding region.");
                return null; // return an empty list instead of null
            }
        }

        /// <summary>
        /// Handles HTTP call to the API for updating a region
        /// </summary>
        /// <param name="updateRegionViewModel"></param>
        /// <returns></returns>
        public async Task<RegionDto> UpdateRegionAsync(UpdateRegionViewModel updateRegionViewModel)
        {
            try
            {
                // 1. get the authorized client
                var client = await _httpClientAuthService.GetAuthorizedClient();

                // 2. Map UpdateRegionViewModel to RegionDto

                var regionDto = _mapper.Map<RegionDto>(updateRegionViewModel);

                // 3. make the request
                //var response = await client.PutAsJsonAsync($"api/v1/regions/{regionDto.Id}", regionDto);
                var response = await ExecuteWithRefreshRetry(() => client.PutAsJsonAsync($"api/v1/regions/{regionDto.Id}", regionDto));

                response.EnsureSuccessStatusCode();

                //if (response.IsSuccessStatusCode)
                //{
                return await response.Content.ReadFromJsonAsync<RegionDto>();
                //    return updatedRegion;
                //}
                //else
                //{
                //    _logger.LogError("Error updating region: {StatusCode}", response.StatusCode);
                //    return null;
                //}
            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while updating region with ID {RegionId}.", updateRegionViewModel.Id);
                return null; // return an empty list of regions or an appropriate error message
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating region with ID {RegionId}.", updateRegionViewModel.Id);
                return null; // return an empty list instead of null
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
                // 1. get the authorized client
                var client = await _httpClientAuthService.GetAuthorizedClient();

                // 2. make the request
                //var response = await client.DeleteAsync($"api/v1/regions/{id}");
                var response = await ExecuteWithRefreshRetry(() => ExecuteWithRefreshRetry(() => client.DeleteAsync($"api/v1/regions/{id}")));

                //if (!response.IsSuccessStatusCode)
                //{
                //    var errorMessage = await response.Content.ReadAsStringAsync();
                //    _logger.LogError("Error deleting region: {StatusCode} - {Content}", response.StatusCode, errorMessage);
                //}

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Region with ID {RegionId} deleted successfully.", id);
                return true;
            }
            catch (HttpRequestException ex)
            {
                // log the exception
                _logger.LogError(ex, "HTTP request error while deleting region with ID {RegionId}.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting region with ID {RegionId}.", id);
            }
            return false;
        }

        private async Task<HttpResponseMessage> ExecuteWithRefreshRetry(Func<Task<HttpResponseMessage>> action)
        {
            var response = await action();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized (401). Attempting token refresh.");

                var newAccessToken = _refreshTokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    _logger.LogInformation("Token refreshed. Retrying request.");
                    return await action(); // Retry the original action
                }

                _logger.LogError("Token refresh failed. Clearing refresh token.");
                await _refreshTokenService.DeleteRefreshToken();
            }

            return response;
        }
    }
}
