using System.ComponentModel.DataAnnotations;

namespace NZWalks.API.Models.DTO
{
    public class UpdateWalkRequestDto
    {
        // we are including the properties that the client can update
        [Required]
        [MaxLength(100, ErrorMessage = "Max length is 100")]
        public string Name { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = "Max length is 1000")]
        public string Description { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(2, ErrorMessage = "Maxlength is 2")]
        //[Range(0,50)] if type double was used
        public double LengthInKm { get; set; }
        public string? WalkImageUrl { get; set; }


        // Foreign Keys
        [Required]
        public Guid DifficultyId { get; set; }

        [Required]
        public Guid RegionId { get; set; }
    }
}
