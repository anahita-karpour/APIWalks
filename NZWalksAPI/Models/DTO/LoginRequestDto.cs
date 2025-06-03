using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class LoginRequestDto
    {
        // This class is used to serialize the request to the API when a user logs in
        // The API expects a JSON object with the following properties
        // This DTO is used to send the user's login credentials from the UI to the API.
        // It should contain the minimal information required for the API to authenticate the user.

        // contains the user's credentials

        [DataType(DataType.EmailAddress)]
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
