using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.Domain
{
    public class Member
    {
        // Individual Member Properties
        public Guid Id { get; set; }

        [Required]
        [EmailAddress] // Ensures the email is in a valid format
        public string Email { get; set; } // This should ideally be synced with IdentityUser.Email

        //[Required]
        //[StringLength(10, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 10 characters.")]
        //public string Password { get; set; }

        public string Role { get; set; } = "User"; // Role of the user, e.g., Admin, User - default is User

        // Foreign key to link this profile to the IdentityUser
        [Required]
        public string IdentityUserId { get; set; } // Stores the string ID of the related IdentityUser

    }
}
