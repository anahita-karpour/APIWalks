using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public interface IRegionRepository
    {
        Task<List<Region>> GetAllAsync();
        Task<Region?> GetRegionByIdAsync(Guid id); //Region can be null so : <Region?>:nullable region
        Task<Region> CreateRegionAsync(Region region);
        Task<Region?> UpdateRegionAsync(Guid id, Region region);
        Task<Region?> DeleteRegionAsync(Guid id); // it is nullable since it will try to return the id,
                                                  // if it can it will return it
                                                  // otherwise it will return null
    }
}
