using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class UpdateMemberRequestDto
    {
        // Individual Member Properties that UI project will send to the API for updating a member
        // Assuming Email, Password, and Role are the properties that can be updated

        [Required(ErrorMessage = "Email is required for update.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 10 characters.")]
        public string? Password { get; set; }  // Nullable Password: means the password field is optional for update operations. If a value is provided,
                                               // it will be used to reset the user's password; otherwise, the password remains unchanged

        [Required(ErrorMessage = "Role is required for update.")]
        public string Role { get; set; } // Expecting "Reader/ Writer" or "Admin"
    }
}
