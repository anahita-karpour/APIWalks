using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace NZWalks.API.Services.Auth
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(IdentityUser user, IEnumerable<string> roles);
        ClaimsPrincipal GetClaimsPrincipalFromToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
