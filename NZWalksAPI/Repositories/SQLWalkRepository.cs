using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class SQLWalkRepository : IWalkRepository
    {
        private readonly NZWalksDbContext _dbContext;

        public SQLWalkRepository(NZWalksDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Walk> CreatAsync(Walk walk)
        {
            await _dbContext.Walks.AddAsync(walk);
            await _dbContext.SaveChangesAsync();
            return walk;
        }

        public async Task<Walk> DeleteAsync(Guid id)
        {
            var existingWalk = await _dbContext.Walks.FirstOrDefaultAsync(x => x.Id == id);

            if (existingWalk == null)
            {
                return null;
            }
            _dbContext.Walks.Remove(existingWalk);
            await _dbContext.SaveChangesAsync();
            return existingWalk;
        }

        public async Task<List<Walk>> GetAllAsync(
            string? filterOn = null, string? filterQuery = null, // Filtering
            string? sortBy = null, bool? isAscending = true,     // Sorting
            int pageNumber = 1, int pageSize = 1000)             // Pagination
        {
            // Walks is the DbSet in the dbContext
            //return await _dbContext.Walks.ToListAsync();

            //return await _dbContext.Walks.Include("Difficulty").Include("Region").ToListAsync(); // Using the Include() method we can
            //get the navigation properties: Difficulty and  for the walks back as well
            //to make the Include() function type safe; you can write the function as follow
            //Include(x => x.Difficulty)

            var walks = _dbContext.Walks.Include("Difficulty").Include("Region").AsQueryable();

            //  filtering
            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                // Name
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = walks.Where(x => x.Name.Contains(filterQuery));
                }

                // Description
                if (filterOn.Equals("Description", StringComparison.OrdinalIgnoreCase))
                {
                    walks = walks.Where(x => x.Description.Contains(filterQuery));
                }

                //// LengthInKm
                //if (filterOn.Equals("LengthInKm", StringComparison.OrdinalIgnoreCase))
                //{
                //    walks = walks.Where(x => x.LengthInKm.Equals(filterQuery));
                //}
            }
            // Sorting
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = (bool)isAscending ? walks.OrderBy(x => x.Name) : walks.OrderByDescending(x => x.Name);
                }
                if (sortBy.Equals("Length", StringComparison.OrdinalIgnoreCase))
                {
                    walks = (bool)isAscending ? walks.OrderBy(x => x.LengthInKm) : walks.OrderByDescending(x => x.LengthInKm);
                }
            }
            // Pagination
            var skipResults = (pageNumber - 1) * pageSize;


            return await walks.Skip(skipResults).Take(pageSize).ToListAsync();

        }
        public async Task<Walk?> GetByIdAsync(Guid id)
        {
            var existingWalk = await _dbContext.Walks
                .Include("Difficulty")
                .Include("Region")
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingWalk == null)
            {
                return null;
            }

            return existingWalk;
        }

        public async Task<Walk?> UpdateAsync(Guid id, Walk walk)
        {
            var existingWalk = await _dbContext.Walks
                .Include("Difficulty")
                .Include("Region")
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingWalk == null)
            {
                return null;
            }

            existingWalk.Name = walk.Name;
            existingWalk.Description = walk.Description;
            existingWalk.LengthInKm = walk.LengthInKm;
            existingWalk.WalkImageUrl = walk.WalkImageUrl;
            existingWalk.DifficultyId = walk.DifficultyId;
            existingWalk.RegionId = walk.RegionId;

            await _dbContext.SaveChangesAsync();
            return existingWalk;

        }
    }
}
