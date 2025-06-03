using System.ComponentModel.DataAnnotations;

namespace NZWalks.UI.Models.ViewModel
{
    public class LoginViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
