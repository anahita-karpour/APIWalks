using Newtonsoft.Json;

namespace NZWalks.UI.Models.DTO
{
    public class LoginResponseDto
    {
        // This class is used to deserialize the response from the API when a user logs in
        // The API returns a JSON object with the following properties
        // This DTO is used to send the API's response to the UI after a login attempt.
        // It should contain the information the UI needs to manage the user's session.

        // contains the authentication token and other session-related information

        [JsonProperty("token")] // Explicitly map to the JSON response property name
        public string Token { get; set; }

        [JsonProperty("refreshToken")] // Map the refresh token property
        public string RefreshToken { get; set; }
        // You might have other properties like a success message, etc.
        // [JsonProperty("message")]
        // public string Message { get; set; }
        //public string Message { get; set; }
        public string Roles { get; set; } // or List<string> Roles { get; set; }
    }
}
//To correctly deserialize the API's response, the UI's LoginResponseDto must mirror the structure of the API's LoginResponseDto.