using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NZWalks.API.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT token for a user with the given roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public string GenerateJwtToken(IdentityUser user, IEnumerable<string> roles)
        {
            // 1. Claims: Create claims from the user
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (user ID)
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // Email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
            };
            //var claims = new List<Claim>();

            //claims.Add(new Claim(ClaimTypes.Email, user.Email)); // Email
            //claims.Add(new Claim(ClaimTypes.UserData, user.Id)); // Subject (user ID)
            //claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Unique token ID


            // Add roles
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim("role", role));
            //}
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Role
            }

            // Add the "area" claim if the user belongs to the Admin area
            if (roles.Contains("Admin")) // Example: Based on the user's role
            {
                claims.Add(new Claim("area", "Admin"));
            }

            // 2. Signing Key: Used to sign the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Secret key from secrete store/configuration
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //
            int expiresInMinuts = _configuration.GetValue<int>("Jwt:AccessTokenDurationMinutes", 30);

            // 3. Token Descriptor: Defines token properties
            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(claims),
            //    Expires = DateTime.UtcNow.AddMinutes(expiresInMinuts), // Token expiration
            //    SigningCredentials = credentials,
            //    Issuer = _configuration["Jwt:Issuer"],
            //    Audience = _configuration["Jwt:Audience"]
            //};
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinuts), // Token expiration
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);


            //// 4. Token Handler: Creates the token
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var token = tokenHandler.CreateToken(tokenDescriptor);

            //// 5. Return the token as a string
            //return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true, // Validate the lifetime of the token
                    // Set clockskew to zero so tokens expire exactly at token expiration time
                    ClockSkew = TimeSpan.Zero // Remove the default 5 minutes clock skew
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception)
            {
                return false; // Token validation failed
            }
        }
        public ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false, // DO NOT VALIDATE LIFETIME HERE
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return principal;
            }
            catch
            {
                return null; // Token validation failed
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false, // Crucial change: Don't validate the lifetime
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                var jwtSecurityToken = validatedToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
