namespace NZWalks.API.Models.DTO
{
    public class LogoutRequestDto
    {
        /// <summary>
        /// The refresh token to be invalidated
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;


        ///// <summary>
        ///// The user ID of the user logging out
        ///// </summary>
        //public string UserId { get; set; } = string.Empty;

    }
}
