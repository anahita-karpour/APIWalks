using NZWalks.API.Models.DTO;

namespace NZWalks.UI.Services.Regions
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionDtoV1>> GetAllRegionsAsync();
        Task<RegionDtoV1> GetRegionByIdAsync(Guid id);

        //-------

        // ViewModel is used for the UI layer, while RequestDto is used for API requests
        // RegionService should ideally work with the APIs DTOs, not the ViewModels directly.
        // The mapping from RegionViewModel to RegionUpdateRequestDto should be handled in the controller.
        Task<RegionDtoV1> AddRegionAsync(AddRegionRequestDto addRegion);

        Task<bool> UpdateRegionAsync(UpdateRegionRequestDto updateRegionDto);
        //------



        Task<bool> DeleteRegionAsync(Guid id);
    }
}
