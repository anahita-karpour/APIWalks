using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class ImageUploadResponseDto
    {
        public Guid Id { get; set; }
        //[Required]
        //public required IFormFile File { get; set; }     // the file that is being sent - the type of this is IFormFile
        [Required]
        public required string FileName { get; set; }
        public string? FileDescription { get; set; }
        public string FilePath { get; set; } // the URL path to the uploaded image file
    }
}
// Request vs. Response:
// The ImageUploadRequestDto includes the IFormFile because that's the data being sent to the server.
// The ImageUploadResponseDto does not include IFormFile because the client doesn't need to receive the raw file data back in the response.
// Instead, it receives metadata like the Id, FileName, FileDescription, and importantly, the FilePath (the URL to access the image).