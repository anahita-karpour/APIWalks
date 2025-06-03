using System.ComponentModel.DataAnnotations.Schema;

namespace NZWalks.API.Models.Domain
{
    public class Image
    {
        // The following is Transient data.
        // Transient data is information that exists temporarily and is relevant only for a limited time, typically during a specific
        // operation or process. It's not intended for long-term storage in its current form

        [NotMapped] // This property is not mapped to the database: prevent EF Core from trying to store the IFormFile in the database
                    // IFormFile is a complex object representing the uploaded file,
                    // and it doesn't directly correspond to simple string or long types of FileExtension and FileSizeInBytes
        public required IFormFile File { get; set; }

        // The following are Persistent metadata. they are descriptive information about the data that is intended for long-term storage
        // and retrieval.
        // It provides context and allows us to manage and access the actual data later
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public string? FileDescription { get; set; }
        public required string FileExtension { get; set; }
        public long FileSizeInBytes { get; set; }
        public required string FilePath { get; set; }
    }
}
