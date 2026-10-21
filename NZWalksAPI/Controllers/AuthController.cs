using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using NZWalks.API.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(UserManager<IdentityUser> userManager, ILogger<AuthController> logger,
            IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService) // Because we injected IdentityUser we get the UserManager to create a new user
        {
            _userManager = userManager;
            _logger = logger;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }


        // POST: /api/Auth/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        //public async Task<IActionResult> Register([FromForm] RegisterRequestDto registerRequestDto)
        {
            // Check if user with the same email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerRequestDto.Username); // Use username, as you are using email as username.

            if (existingUser != null)
            {
                return BadRequest(new { Message = "Email address is already in use." });
                // creates an anonymous object with a single property named Message
                // message is sent back to the client in the HTTP response body
                // and an HTTP 400 Bad Request response to the client.
            }

            _logger.LogInformation("Register attempt for user {registerRequestDto.Username}", registerRequestDto.Username); // Add this line
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username
            };
            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors); // Return detailed errors
                                                          // return an HTTP 400 Bad Request response to the client.
            }

            // Add roles to this user
            if (!string.IsNullOrEmpty(registerRequestDto.Roles))
            {
                _logger.LogInformation($"Attempting to add role: {registerRequestDto.Roles}"); // Log the role

                // Add roles to this user
                // identityResult = await _userManager.AddToRolesAsync(identityUser, new string[] { registerRequestDto.Role });
                //identityResult = await _userManager.AddToRoleAsync(identityUser, "reader");
                //identityResult = await _userManager.AddToRolesAsync(identityUser, registerRequestDto.Roles);
                identityResult = await _userManager.AddToRoleAsync(identityUser, registerRequestDto.Roles);
                if (!identityResult.Succeeded)
                {
                    return BadRequest(identityResult.Errors); // Return detailed errors
                }
            }

            // Create a JSON object for the response
            var responseObject = new
            {
                Message = "User was registered successfully. Please login.",
                Roles = registerRequestDto.Roles
            };
            return Ok(responseObject);
        }


        // Fix for CS0136 and CS8600
        // The issue with CS0136 is that the variable name 'role' is already used in an enclosing scope.
        // The issue with CS8600 is that `roles.SingleOrDefault()` might return null, and assigning it to a non-nullable type causes the warning.

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Username);

            if (user != null)
            {
                // Check if the password is correct
                var checkPasswordResult = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (checkPasswordResult)
                {
                    // Get the roles for this user
                    var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                    // Use a different variable name to avoid CS0136
                    string userRole = roles.SingleOrDefault() ?? string.Empty; // Handle null to avoid CS8600

                    Console.WriteLine($"User Role: {userRole}");
                    _logger.LogInformation($"User Role: {userRole}");
                    //if (roles != null)
                    //{
                    // Generate the JWT token
                    var jwtTokenString = _jwtTokenService.GenerateJwtToken(user, roles.ToList());

                    // Create a new JWT token handler
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(jwtTokenString);

                    // Get the jti claim
                    var jwtId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                    //}


                    if (string.IsNullOrEmpty(jwtId))
                    {
                        return BadRequest("JWT does not contain jti claim.");
                    }

                    // Generate a new refresh token
                    var refreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, jwtId);

                    var loginResponseDto = new LoginResponseDto
                    {
                        Token = jwtTokenString,
                        RefreshToken = refreshToken.Token,
                        Roles = userRole // Use the updated variable name
                    };

                    return Ok(loginResponseDto);
                }
            }

            return BadRequest("Username or password was incorrect.");
        }


        // POST: /api/Auth/Logout
        // This endpoint is used to log out a user by invalidating their refresh token.
        [HttpPost]
        [Route("Logout")]
        [Authorize] // Require authentication to logout

        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutRequestDto)
        {
            if (logoutRequestDto?.RefreshToken == null)
            {
                return BadRequest("Refresh Token is required for logout.");
            }

            var refreshToken = logoutRequestDto.RefreshToken;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Should have a user if [Authorize] is used
            }

            var storedRefreshToken = await _refreshTokenService.GetRefreshToken(refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.UserId != userId || storedRefreshToken.Invalidated || storedRefreshToken.Used)
            {
                return BadRequest("Invalid Refresh Token for logout.");
            }

            // Invalidate the refresh token
            var invalidated = await _refreshTokenService.InvalidateRefreshToken(refreshToken);

            if (invalidated)
            {
                return Ok(new { Message = "Logged out successfully." });
            }
            else
            {
                _logger.LogError("Failed to invalidate refresh token for user '{UserId}'.", userId);
                return StatusCode(500, "Internal Server Error: Failed to invalidate refresh token.");
            }
        }

        // POST: /api/Auth/RefreshToken
        [HttpPost]
        [Route("RefreshToken")]

        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
        {
            _logger.LogInformation("Processing refresh token request."); // Log the start
            // 1. Request Handling: the [FromBody] attribute indicates that the data (AccessToken and RefreshToken) will be sent in the JSON body of the HTTP request
            // use RefreshTokenRequestDto to recieve the incoming data: the access token and refresh token
            // use the RefreshTokenResponseDto to send back the new tokens in response back to the client
            try
            {
                // 2. Input validation: Checks if both AccessToken and RefreshToken are provided in the request
                if (string.IsNullOrEmpty(refreshTokenRequestDto?.AccessToken) || string.IsNullOrEmpty(refreshTokenRequestDto?.RefreshToken))
                {
                    _logger.LogWarning("BadRequest: Access Token or Refresh Token is missing.");
                    return BadRequest("Access Token and Refresh Token are required.");
                }
                try
                {
                    //3.Access Token Validation: Extract claims from the AccessToken without validating its lifetime.
                    //This is essential to get the jwtId and userId associated with the token.
                    var principal = _jwtTokenService.GetPrincipalFromExpiredToken(refreshTokenRequestDto.AccessToken);

                    if (principal == null)
                    {
                        _logger.LogWarning("BadRequest: Invalid Access Token.");
                        return Unauthorized("Invalid Access Token.");
                    }

                    var jwtId = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                    var userId = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(jwtId) || string.IsNullOrEmpty(userId))
                    {
                        _logger.LogWarning("BadRequest: Invalid Access Token Claims - Missing UserId or JwtId.");
                        return BadRequest("Invalid Access Token Claims.");
                    }


                    // 4. Refresh Token Validation: Check if the refresh token is valid and associated with the user.
                    // Get the refresh token from the database using the refresh token string
                    // Ensures the refresh token exists.
                    // Verifies that the JwtId in the refresh token matches the jwtId from the access token.
                    // Check if the UserId in the refresh token matches the userId from the access token.
                    // Checks if the refresh token has been invalidated or used.
                    // Ensures the refresh token has not expired.
                    var storedRefreshToken = await _refreshTokenService.GetRefreshToken(refreshTokenRequestDto.RefreshToken);
                    _logger.LogInformation("Stored refresh token found: {Found}.", storedRefreshToken != null);


                    if (storedRefreshToken == null ||
                        storedRefreshToken.JwtId != jwtId ||
                        storedRefreshToken.UserId != userId ||
                        storedRefreshToken.Invalidated ||
                        storedRefreshToken.Used ||
                        storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                    {
                        if (storedRefreshToken != null && storedRefreshToken.ExpiryDate >= DateTime.UtcNow && !storedRefreshToken.Invalidated && !storedRefreshToken.Used)
                        {
                            // Only invalidate if it hasn't already been invalidated or used
                            await _refreshTokenService.InvalidateRefreshToken(refreshTokenRequestDto.RefreshToken);
                        }
                        _logger.LogWarning("Unauthorized: Invalid or expired refresh token.");
                        return Unauthorized("Invalid or expired Refresh Token.");
                    }

                    // 5. Refresh Token Update: Mark the old refresh token 
                    await _refreshTokenService.UpdateRefreshToken(new RefreshToken
                    {
                        Id = storedRefreshToken.Id,
                        Token = storedRefreshToken.Token,
                        JwtId = storedRefreshToken.JwtId,
                        CreationDate = storedRefreshToken.CreationDate,
                        ExpiryDate = storedRefreshToken.ExpiryDate,
                        Used = true,
                        Invalidated = storedRefreshToken.Invalidated,
                        UserId = storedRefreshToken.UserId
                    });


                    // 6. Generate New Tokens: Create a new JWT token and associated with the user and the jwtId of the new access token.
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return NotFound("User associated with the refresh token not found.");
                    }
                    var roles = await _userManager.GetRolesAsync(user);

                    var newJwtToken = _jwtTokenService.GenerateJwtToken(user, roles);
                    var newRefreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, new JwtSecurityTokenHandler().ReadJwtToken(newJwtToken).Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value);


                    // 7. Response: returns New Tokens: a 200 OK response with a RefreshTokenResponseDto containing the Token (new access token)
                    // and the RefreshToken (new refresh token).
                    return Ok(new RefreshTokenResponseDto
                    {
                        Token = newJwtToken,
                        RefreshToken = newRefreshToken.Token
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing token.");
                    return StatusCode(500, "Internal Server Error.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshToken endpoint.");
                return StatusCode(500, "Internal Server Error.");
            }

        }

    }
}


