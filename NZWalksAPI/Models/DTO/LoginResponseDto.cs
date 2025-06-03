using Newtonsoft.Json;

namespace NZWalks.API.Models.DTO
{
    public class LoginResponseDto
    {
        // returns both the access token and the refresh token information

        [JsonProperty("token")] // Explicitly map to the JSON response property name
        public string Token { get; set; }

        [JsonProperty("refreshToken")] // Map the refresh token property
        public string RefreshToken { get; set; }

        public string Roles { get; set; } // or List<string> Roles{get; set;}
    }
}
