using System.ComponentModel.DataAnnotations;

namespace NZWalks.UI.Areas.Admin.Models.ViewModel
{
    // Model tailored for Razor page(View); these ViewModels will interact with the API's DTOs
    public class MembersViewModel
    {
        // Individual Member Properties
        public Guid UserId { get; set; }
        [Required]
        [EmailAddress] // Ensures the email is in a valid format
        public string Email { get; set; }

        [Display(Name = "New Password (optional)")] // Password is optional for updates. Only provides if changing.
        [StringLength(10, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 10 characters.")]
        public string? Password { get; set; } // Marked as nullable for optional updates. Only provides if changing.

        public string Role { get; set; } = "User"; // Role of the user, e.g., Admin, User - default is User

        // Properties for the "Members List" section (right column - table)
        public List<MemberListItem> Members { get; set; } = new List<MemberListItem>();


    }

    // --- Helper class for displaying members in the table ---
    public class MemberListItem
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}

//public string? Password { get; set; }:
//This is specifically for when the user submits a new password.
//When we fetch an existing user for display,
//we should never populate this Password property with the actual password from your database (which should be hashed anyway).
//It's best to leave it null or empty when initially loading a member's profile for editing to avoid accidentally exposing sensitive data.
//The [Display(Name = "New Password (optional)")] implies this is for input.