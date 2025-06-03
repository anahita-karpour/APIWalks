using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class ImageUploadRequestDto
    {
        [Required]
        public required IFormFile File { get; set; }     // the file that is being sent - the type of this is IFormFile
                                                         // The IFormFile interface is used to handle file uploads in
                                                         // ASP.NET Core applications.
        [Required]
        public required string FileName { get; set; }
        public string? FileDescription { get; set; }        // can be null

    }
}
