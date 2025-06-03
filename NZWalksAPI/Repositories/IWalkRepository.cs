using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public interface IWalkRepository
    {
        Task<Walk> CreatAsync(Walk walk);
        Task<List<Walk>> GetAllAsync(
            string? filterOn = null, string? filterQuery = null,  // Filtering
            string? sortBy = null, bool? isAscending = true,      // Sorting
            int pageNumber = 1, int pageSize = 1000);             // Pagination 

        Task<Walk?> GetByIdAsync(Guid id);
        Task<Walk?> UpdateAsync(Guid id, Walk walk);
        Task<Walk?> DeleteAsync(Guid id);
    }
}
