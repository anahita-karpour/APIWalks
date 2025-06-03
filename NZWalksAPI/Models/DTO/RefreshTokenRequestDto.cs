namespace NZWalks.API.Models.DTO
{
    public class RefreshTokenRequestDto
    {
        // access token and refresh token are sent in the request body
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
