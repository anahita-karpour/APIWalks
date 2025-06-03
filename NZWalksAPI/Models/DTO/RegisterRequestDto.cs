using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class RegisterRequestDto
    {
        //[Required]
        [DataType(DataType.EmailAddress)]
        public required string Username { get; set; }

        //[Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        //public string[] Roles { get; set; } // or List<string> Roles{get; set;}
        public string Roles { get; set; }
    }
}
