using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NZWalks.UI.Models.ViewModel
{
    public class UpdateRegionViewModel
    {
        [ReadOnly(true)]
        public Guid Id { get; set; } // Unique identifier for the region

        [Required(ErrorMessage = "Code is required.")]
        [StringLength(5, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 5 characters.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public string? RegionImageUrl { get; set; } // Nullable if image is optional
    }
}
