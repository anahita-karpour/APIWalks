using System.ComponentModel.DataAnnotations;

namespace NZWalks.UI.Models.DTO
{
    public class RegionDto
    {
        //properties that you want to expose to the client
        public Guid Id { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Code has to be a minimum of 3 characters")]
        [StringLength(3, ErrorMessage = "Code has to be a maximum of 3 characters")]
        public string Code { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name has to be a maxium of 100 characters")]
        public string Name { get; set; }
        public string RegionImageUrl { get; set; } //string? means a nullable string type

    }
}
