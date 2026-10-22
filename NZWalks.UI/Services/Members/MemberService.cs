using NZWalks.API.Models.DTO;
using NZWalks.UI.Services.Auths;

namespace NZWalks.UI.Services.Members
{
    public class MemberService : BaseApiService, IMemberService
    {
        private readonly IConfiguration _configuration;

        public MemberService(IHttpClientAuthService httpClientAuthService,
            ILogger<MemberService> logger,
            UI.Services.Auths.IRefreshTokenService refreshTokenService,
            IConfiguration configuration,
            IJwtTokenServiceUI jwtTokenServiceUI
            ) : base(logger, jwtTokenServiceUI, refreshTokenService, httpClientAuthService) // Call the base constructor

        {
            _configuration = configuration;
        }


        /// <summary>
        /// Gets all members from the API
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(() =>
                    _httpClientAuthService.SendAuthorizedRequest(HttpMethod.Get, "api/members"));

                response.EnsureSuccessStatusCode();

                var members = await response.Content.ReadFromJsonAsync<IEnumerable<MemberDto>>();
                return members ?? new List<MemberDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching all members.");
                return new List<MemberDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching all members.");
                return new List<MemberDto>();
            }
        }

        /// <summary>
        /// Gets a single member by ID for pre-populating the form
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MemberDto?> GetMemberByIdAsync(Guid id)
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(() =>
                        _httpClientAuthService.SendAuthorizedRequest(HttpMethod.Get, $"api/members/{id}"));/*$"{ApiBaseUrl}/{id}"));*/

                response.EnsureSuccessStatusCode();

                var member = await response.Content.ReadFromJsonAsync<MemberDto>();

                return member;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching member with ID {MemberId}.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching member with ID {MemberId}.", id);
                return null;
            }
        }

        public async Task<bool> UpdateMemberAsync(Guid id, UpdateMemberRequestDto memberUpdateRequestDto)
        {
            try
            {
                var response = await ExecuteWithRefreshRetry(() =>
                    _httpClientAuthService.SendAuthorizedRequest(HttpMethod.Put, $"api/members/{id}", memberUpdateRequestDto));

                response.EnsureSuccessStatusCode(); // if this line doesn't throw, it is successful
                return true; // so just return true
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while updating member with ID {MemberId}.", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating member with ID {MemberId}.", id);
                return false;
            }
        }
        public async Task<bool> DeleteMemberAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid member ID provided for deletion.");
                    return false;
                }

                var response = await ExecuteWithRefreshRetry(async () =>
                {
                    // _httpClientAuthService.SendAuthorizedRequest handles getting the client internally
                    return await _httpClientAuthService.SendAuthorizedRequest(HttpMethod.Delete, $"api/members/{id}");
                });
                // 3. check the response
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Member with ID {MemberId} deleted successfully.", id);
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while deleting member with ID {MemberId}.", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting member with ID {MemberId}.", id);
                return false;
            }
        }

    }
}