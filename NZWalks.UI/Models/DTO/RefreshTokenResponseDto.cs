namespace NZWalks.UI.Models.DTO
{
    public class RefreshTokenResponseDto
    {
        // This class is used to deserialize the response from the API when a user refreshes their token
        // The API returns a JSON object with the following properties
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        //public string Message { get; set; }
    }
}
