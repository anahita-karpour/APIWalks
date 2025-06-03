namespace NZWalks.API.Models.DTO
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        // Add other properties if your API returns more than just the token        
    }
}
