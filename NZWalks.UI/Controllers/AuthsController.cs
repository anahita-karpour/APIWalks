using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NZWalks.UI.Models.DTO;
using NZWalks.UI.Models.ViewModel;
using NZWalks.UI.Services.Auths;
using NZWalks.UI.Services.Common;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace NZWalks.UI.Controllers
{
    [Route("[controller]")]
    public class AuthsController : Controller
    {

        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthsController> _logger;
        private readonly ICookieService _cookieService;
        private readonly IRefreshTokenService _refreshTokenService;

        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly SignInManager<IdentityUser> _signInManager;

        //public AuthsController(IHttpClientFactory httpClient, IConfiguration configuration, ILogger<AuthsController> logger,
        //   SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        public AuthsController(IHttpClientFactory httpClient, IConfiguration configuration, ILogger<AuthsController> logger,
            ICookieService cookieService, IRefreshTokenService refreshTokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cookieService = cookieService;
            _refreshTokenService = refreshTokenService;

            //_userManager = userManager;
            //_signInManager = signInManager;
            //
            // Inject IHttpClientFactory to create HttpClient instances for API calls.
            // Inject IConfiguration to access your API's base URL from appsettings.json.
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            try
            {
                // Server-side role validation
                if (registerViewModel.Roles != "Reader" && registerViewModel.Roles != "Writer") //&& registerViewModel.Roles != "admin")
                {
                    ModelState.AddModelError("Roles", "Invalid role selected.");
                    return View(registerViewModel);
                }

                // Create RegisterRequestDto
                var registerRequestDto = new RegisterRequestDto
                {
                    Username = registerViewModel.Username,
                    Password = registerViewModel.Password,
                    Roles = registerViewModel.Roles
                };

                // 1- use the HttpClientFactory to create an HttpClient instance to send requests
                //var client = _httpClient.CreateClient();
                var client = _httpClient.CreateClient("NZWalksAPI");

                // 2- Create the request message
                //var json = System.Text.Json.JsonSerializer.Serialize(registerViewModel);
                var json = JsonConvert.SerializeObject(registerRequestDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/auth/register";

                _logger.LogInformation("Sending Register Request: {Url}", url);

                var httpRequestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = content
                };

                _logger.LogInformation("Sending Register Request: {RequestUri}", httpRequestMessage.RequestUri);

                //3 - Send the request
                //var httpResponseMessage = await client.SendAsync(httpRequestMessage);

                //var response = await client.PostAsync("/api/auths/register", content);
                var response = await client.PostAsync(url, content);
                _logger.LogInformation("Received Register Response: {StatusCode}, {ResponseContent}", response.StatusCode, await response.Content.ReadAsStringAsync());

                //4 - Handle the API response
                if (response.IsSuccessStatusCode)
                {
                    // API returns a JSON object with Message and Roles
                    //// 5- Store the authentication token (JWT) in the local storage or session storage
                    // For login, after getting the token, you will need to setup authentication in the UI. 
                    // This is often done by storing the token in local storage, and then adding the token to the authorization header of each request.

                    var responseContent = await response.Content.ReadAsStringAsync();
                    //var responseObject = System.Text.Json.JsonSerializer.Deserialize<RegisterResponseDto>(responseContent);
                    var responseObject = JsonConvert.DeserializeObject<RegisterResponseDto>(responseContent);


                    if (responseObject != null)
                    {
                        _logger.LogInformation("Redirecting to Login page after successful registration.");

                        // Add success message to TempData
                        // TempData is a dictionary that stores data.  persists data for a single subsequent request,
                        // making it perfect for passing messages between redirects.
                        TempData["SuccessMessage"] = "Registration successful! Please log in.";

                        return RedirectToAction("Login", "Auths"); // Redirect to login
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Registration successful, but response was unexpected.");
                        return View(registerViewModel);
                    }
                }
                else
                {
                    // Handle API errors
                    var errorContent = await response.Content.ReadAsStringAsync(); // the code accesses the content of the HTTP
                                                                                   // response received from your API's Register endpoint
                    try
                    {                                                               //var errorObject = System.Text.Json.JsonSerializer.Deserialize<RegisterResponseDto>(errorContent);
                        var errorObject = JsonConvert.DeserializeObject<RegisterResponseDto>(errorContent);
                        // The above method is a method from the Newtonsoft.Json library
                        // Deserialization is the process of converting a JSON string back into a C# object.
                        // new{Message=""}: This creates an anonymous C# object with a single property named Message.
                        // JsonConvert.DeserializeAnonymousType() takes the JSON string from errorContent and converts
                        // it into a C# anonymous object, and stores that object in the errorObject variable.
                        //The property called message from the Json string is stored in the message property
                        //of the newly created anonymous object called errorObject.


                        if (errorObject != null && errorObject.Message == "Email address is already in use.")
                        {
                            ModelState.AddModelError("Username", errorObject.Message); // Display error on the Username field.
                            return View(registerViewModel);
                        }

                        _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                        return View(registerViewModel);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, $"API Error: {response.StatusCode} - {errorContent}");
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                        return View(registerViewModel);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error adding User.");
                ModelState.AddModelError(string.Empty, "An error occurred during registration.");
                return View(registerViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(registerViewModel);
            }
        }

        /// <summary>
        /// This action method handles GET requests to the Login page.
        /// It handles the rendering of the login page (or the logic that is executed when application loads the login page)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            //To check if  the session is cleared after logout
            if (HttpContext.Session.GetString("AuthToken") != null)
            {
                // This should be null after a successful logout
                _logger.LogInformation("AuthToken still exists in session after (supposed) logout!");
            }
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                // This should also be null
                _logger.LogInformation($"UserId still exists in session: {HttpContext.Session.GetInt32("UserId")}");
            }

            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            // API: The API's Login action now only validates credentials.
            // UI: The UI's Login action calls the API, and if successful, uses SignInManager to create the session.
            // HttpClient: The UI now uses HttpClient to communicate with the API.

            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            try
            {

                // 1. Map LoginViewModel to LoginRequestDto
                var loginRequestDto = new Models.DTO.LoginRequestDto
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                };

                //// 2. Pass LoginRequestDto to PostAsJsonAsync
                //var response = await _httpClient.CreateClient("NZWalksAPI").PostAsJsonAsync("/api/Auth/Login", loginRequestDto);
                var client = _httpClient.CreateClient("NZWalksAPI");

                //var json = System.Text.Json.JsonSerializer.Serialize(loginViewModel);
                var json = JsonConvert.SerializeObject(loginRequestDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/auth/login";

                //var response = await client.PostAsync("/api/auth/login", content);
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    //var user = await _userManager.FindByEmailAsync(loginViewModel.Username);
                    //await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    //return RedirectToAction("Index", "Home"); // Or wherever you want to redirect
                    // Successful login
                    /////var token = await response.Content.ReadAsStringAsync(); // Assuming the API returns the token as a string
                    // Store the token in local storage or session storage
                    ////HttpContext.Session.SetString("AuthToken", token);
                    // 1. Extract the JWT token from the response
                    var responseContent = await response.Content.ReadAsStringAsync();

                    //var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(responseContent);
                    var loginResponse = JsonConvert.DeserializeObject<API.Models.DTO.LoginResponseDto>(responseContent);
                    _logger.LogInformation("Login successful for API.");


                    //var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);
                    //var token = tokenResponse.Token;
                    //var refreshToken = tokenResponse.RefreshToken;

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        //string jwtToken = loginResponse.Token;
                        //string refreshToken = loginResponse.RefreshToken;


                        // 2. Store the JWT token in a cookie
                        //var cookieOptions = new CookieOptions
                        _cookieService.SetCookie("jwtToken", loginResponse.Token, new CookieOptions
                        {
                            HttpOnly = true, // Recommended for security
                            Secure = true, // Required for HTTPS
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(30) // Set an appropriate expiration
                        });



                        //Response.Cookies.Append("jwtToken", token, cookieOptions);
                        await _refreshTokenService.StoreRefreshToken(loginResponse.RefreshToken);

                        // 3. Store the refresh token in another secure cookie
                        if (!string.IsNullOrEmpty(loginResponse.RefreshToken))
                        {
                            _cookieService.SetCookie("refreshToken", loginResponse.RefreshToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddDays(7) // Longer expiration for refresh token
                            });
                        }

                        // --Authentication Logic: 
                        // With this updated Login action, after a successful login with the API, the UI will now establish
                        // an authenticated session using cookies managed by the ASP.NET Core authentication middleware.
                        // The browser will receive an authentication cookie, and on subsequent requests, the middleware will
                        // use this cookie to identify the authenticated user.


                        //create a JwtSecurityTokenHandler to read the jwtToken
                        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var rawToken = loginResponse.Token;
                        var token = tokenHandler.ReadJwtToken(rawToken);
                        var claims = token.Claims.ToList();

                        //==============================
                        // If the "area" Claim is in the JWT, Ensure it's Being Added to the ClaimsPrincipal
                        // Check if the "roles" Claim is in the JWT (note the lowercase 's')
                        var rolesClaimFromJwt = token.Claims.FirstOrDefault(c => c.Type.ToLower() == "roles");

                        if (rolesClaimFromJwt != null)
                        {
                            // Split the roles if the API returns them as a comma-separated string
                            var roles = rolesClaimFromJwt.Value.Split(',');
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role.Trim())); // Add as ClaimTypes.Role
                            }
                        }
                        // Remove the original "roles" claim if you only want to use ClaimTypes.Role
                        claims.RemoveAll(c => c.Type.ToLower() == "roles");

                        //var roleClaims = token.Claims.Where(c => c.Type.ToLower() == "role");
                        //foreach (var roleClaim in roleClaims)
                        //{
                        //    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                        //}
                        //foreach (var roleClaim in roleClaims)
                        //{
                        //    claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
                        //}

                        // Check if the "area" claim exists in the JWT
                        var areaClaimFromJwt = token.Claims.FirstOrDefault(c => c.Type.ToLower() == "area");
                        if (areaClaimFromJwt != null)
                        {
                            claims.Add(new Claim("area", areaClaimFromJwt.Value)); // Add the "area" claim
                        }

                        //=================================


                        // Manually map the "role" claim if needed
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        var userEmail = User.FindFirstValue(ClaimTypes.Email); // From the token payload
                        var userRole = User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");


                        //Console.WriteLine("========================================");
                        ////--------------------To see the roles in Cookie
                        //var claimsPrincipal = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        //if (claimsPrincipal?.Principal is ClaimsPrincipal userPrincipal)
                        //{
                        //    var roleClaims = userPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                        //    Console.WriteLine($"Roles in Cookie: {string.Join(", ", roleClaims)}");

                        //    var allClaims = userPrincipal.Claims.Select(c => new { c.Type, c.Value }).ToList();
                        //    foreach (var claim in allClaims)
                        //    {
                        //        Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                        //    }
                        //}
                        //Console.WriteLine("=======================================");
                        ////----------

                        // Add success message to TempData
                        // TempData is a dictionary that stores data.  persists data for a single subsequent request,
                        // making it perfect for passing messages between redirects.
                        //ViewData["Email"] = userEmail;
                        //ViewData["Role"] = userRole;
                        TempData["SuccessMessage"] = $"Login was successful with user email: {ViewData["Email"]} and role: {ViewData["Role"]}";

                        //// To check and verify session clearing
                        //HttpContext.Session.SetString("AuthToken", "some_auth_value");
                        //HttpContext.Session.SetInt32("UserId", 123);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Handle API errors
                        _logger.LogError("loginResponse or LoginResponse.Tokenis null or empty");
                        ModelState.AddModelError(string.Empty, "Login failed: Invalid response from the server.");
                        return View(loginViewModel);
                    }
                }
                else
                {
                    // Handle API errors
                    // Improved error handling for non-success status codes
                    try
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        //var errorObject = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(errorContent); // Assuming RegisterResponseDto contains error message
                        var errorObject = JsonConvert.DeserializeObject<API.Models.DTO.LoginResponseDto>(errorContent); // Assuming RegisterResponseDto contains error message
                        //var errorMessage = errorObject?.Message ?? $"API Error: {response.StatusCode} - {errorContent}";
                        ModelState.AddModelError(string.Empty, $"Login failed");
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(string.Empty, $"Login failed: API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                    return View(loginViewModel);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during login.");
                ModelState.AddModelError(string.Empty, "An error occurred.");
                return View(loginViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(loginViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 0- Get the refresh token from the cookie
            string refreshToken = _cookieService.GetCookie("refreshToken");
            string jwtToken = _cookieService.GetCookie("jwtToken"); // Retrieve the JWT token

            // 1- Call the API Logout endpoint to invalidate the refresh token
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    var client = _httpClient.CreateClient("NZWalksAPI"); // Ensure you have a named HttpClient configured

                    // Add the JWT token to the Authorization header
                    if (!string.IsNullOrEmpty(jwtToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    }

                    var logoutRequest = new { RefreshToken = refreshToken };
                    // Handle the API response
                    var response = await client.PostAsJsonAsync("/api/Auth/Logout", logoutRequest); // Adjust the URL if needed

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"API Logout failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        // Optionally handle the failure (e.g., show a message to the user)
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling API Logout.");
                    // Optionally handle the error
                }
            }
            else
            {
                _logger.LogWarning("No refresh token found to send to the API for logout.");
            }

            // 2- Remove Client-Side Cookies: the JWT token and refresh token cookies
            _cookieService.DeleteCookie("jwtToken");
            _cookieService.DeleteCookie("refreshToken");

            //// 3- Clear the session data
            //HttpContext.Session.Remove("AuthToken");
            //HttpContext.Session.Clear();

            // 3- Explicitly Sign Out of the UI's Authentication Scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 4- Redirect to the login/Index page
            return RedirectToAction("Login", "Auths");
        }

        [HttpGet]
        [Route("AccessDenied")]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

    }
}
