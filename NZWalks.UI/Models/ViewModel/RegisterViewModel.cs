using System.ComponentModel.DataAnnotations;

namespace NZWalks.UI.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public required string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Passwords must be at least 8 characters long and contain uppercase, lowercase, numbers, and special characters.")]]
        public required string Password { get; set; }

        //[Required(ErrorMessage = "At least one role must be selected.")]
        //public required string[] Roles { get; set; } // or List<string> Roles{get; set;}
        [Required(ErrorMessage = "A role must be selected.")]
        public string Roles { get; set; }

        // Optional: Password Confirmation
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")] // Compare ConfirmPassword property with the
                                                                       // Password property and displays the specified
                                                                       // error message if they don't match.
                                                                       //The API does not need to validate the confirm password.
                                                                       //This is a UI concern.
        public required string ConfirmPassword { get; set; }
    }

}
