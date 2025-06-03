using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Services.Auth;
using System.Security.Cryptography;

namespace NZWalks.API.Repositories
{
    public class SQLRefreshTokenRepository : IRefreshTokenService
    {
        private readonly ILogger _logger;
        private readonly NZWalksAuthDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private const int RefreshTokenByteSize = 64;

        public SQLRefreshTokenRepository(ILogger<SQLRefreshTokenRepository> logger, NZWalksAuthDbContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a new refresh token for the user and Jwt ID.
        /// It returns a Task<RefreshToken> representing the generated refresh token.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="jwtId"></param>
        /// <returns></returns>
        public async Task<RefreshToken> GenerateRefreshToken(string userId, string jwtId)
        {

            // Use GetValue<int> to read the duration, providing a key and a default value.
            // It handles parsing and default assignment internally.
            const int defaultDurationMonths = 6;
            int durationMonths = _configuration.GetValue<int>("Jwt:RefreshTokenDurationMonths", defaultDurationMonths);

            // Optional: Log a warning if the default value was used because the config was missing/invalid
            if (durationMonths == defaultDurationMonths && _configuration["Jwt:RefreshTokenDurationMonths"] == null)
            {
                _logger?.LogWarning("Configuration key 'Jwt:RefreshTokenDurationMonths' not found. Using default value of {DefaultDuration} months.", defaultDurationMonths);
            }
            else if (durationMonths == defaultDurationMonths && !int.TryParse(_configuration["Jwt:RefreshTokenDurationMonths"], out _))
            {
                _logger?.LogWarning("Configuration key 'Jwt:RefreshTokenDurationMonths' value '{ConfigValue}' is not a valid integer. Using default value of {DefaultDuration} months.", _configuration["Jwt:RefreshTokenDurationMonths"], defaultDurationMonths);
            }


            // Calculate the actual expiry date using the retrieved or default duration
            var expiryDate = DateTime.UtcNow.AddMonths(durationMonths);

            using (var rng = RandomNumberGenerator.Create())
            {
                // Generate a random token
                var randomBytes = new byte[64];
                rng.GetBytes(randomBytes);
                var refreshToken = new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    JwtId = jwtId,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = expiryDate,//DateTime.UtcNow.AddMonths(6), // Adjust as needed
                    Used = false,
                    Invalidated = false,
                    UserId = userId
                };
                await _dbContext.RefreshTokens.AddAsync(refreshToken);
                await _dbContext.SaveChangesAsync();
                return refreshToken;
            }
        }

        /// <summary>
        /// Retrieves a refresh token from the database based on its token string.
        /// It returns a Task<RefreshToken> representing the found refresh token, or null if not found.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<RefreshToken> GetRefreshToken(string token)
        {
            // Check if the token exists in the database
            // If it does, return the token
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Invalidates a refresh token. typically by marking it as invalid in storage
        /// returns a Task<bool> that is true if successful, and false if not.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> InvalidateRefreshToken(string token)
        {
            var refreshToken = await GetRefreshToken(token);
            if (refreshToken == null) return false;

            refreshToken.Invalidated = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            return await UpdateRefreshToken(refreshToken);
        }

        /// <summary>
        /// Updates an existing refresh token in the database.
        /// It returns a Task<bool> that is true if successful, and false if not
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateRefreshToken(RefreshToken refreshToken)
        {
            // find the existing RefreshToken entity in the database context using its primary key (Id)
            // FindAsync will first check the change tracker and then query the database if the entity is not already being tracked
            var existingToken = await _dbContext.RefreshTokens.FindAsync(refreshToken.Id);

            // If existingToken is null, it means the RefreshToken with the given Id doesn't exist in the database (which shouldn't
            // happen in the context of updating, but it's good practice to handle)
            if (existingToken == null)
            {
                return false; // Or handle the case where the token doesn't exist
            }

            // Update the properties of the tracked entity
            // we are modifying the properties of the entity that is already being tracked.
            existingToken.Token = refreshToken.Token;
            existingToken.JwtId = refreshToken.JwtId;
            existingToken.CreationDate = refreshToken.CreationDate;
            existingToken.ExpiryDate = refreshToken.ExpiryDate;
            existingToken.Used = refreshToken.Used;
            existingToken.Invalidated = refreshToken.Invalidated;
            existingToken.UserId = refreshToken.UserId;

            // Save the changes to the database
            // SaveChangesAsync will save all changes made in the context to the database asynchronously
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
