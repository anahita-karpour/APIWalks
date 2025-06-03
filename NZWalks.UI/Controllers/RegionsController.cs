using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.UI.Models.DTO;
using NZWalks.UI.Models.ViewModel;
using NZWalks.UI.Services.Auths;
using NZWalks.UI.Services.Regions;

namespace NZWalks.UI.Controllers
{
    [Route("[controller]")]
    public class RegionsController : BaseController
    {
        private readonly RegionApiService _regionApiService;
        private readonly ILogger<RegionsController> _logger;
        private readonly IJwtTokenServiceUI _jwtTokenServiceUI;

        /// <summary>
        /// This constructor initializes the RegionsController with the required services.
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public RegionsController(RegionApiService regionApiService, ILogger<RegionsController> logger, IJwtTokenServiceUI jwtTokenServiceUI)
        {
            _regionApiService = regionApiService;
            _logger = logger;
            _jwtTokenServiceUI = jwtTokenServiceUI;
        }


        /// <summary>
        /// This method handles HTTP GET requests to the /Index endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Index")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var regions = await _regionApiService.GetAllRegionsAsync();
                if (regions != null)
                {
                    return View(regions);
                }
                return View(new List<RegionDto>()); // Return an empty list if regions is null.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                return View(new List<RegionDto>());
            }
        }

        /// <summary>
        /// This method handles HTTP GET requests to the /Add endpoint.
        /// </summary>
        /// <returns></returns>

        [HttpGet]  // This method handles HTTP GET requests to the /Add endpoint.
                   // It's responsible for displaying the initial "Add Region" form to the user.
                   // It returns a View(), which renders the Razor view associated with this action.
        [Route("Add")] //Matches /Regions/Add
        [Authorize(Roles = "Admin, Writer")]
        public IActionResult Add()
        {
            // Create a new AddRegionViewModelWithErrorMessage and pass it to the view
            var viewModel = new AddRegionViewModelWithErrorMessage
            {
                RegionViewModel = new AddRegionViewModel() // Initialize AddRegionViewModel
            };
            return View(viewModel);
        }

        [HttpPost] // This method handles HTTP POST requests to the /Add endpoint.
                   // It's responsible for processing the form data submitted by the user.
                   // It receives an instance of the AddRegionViewModel class as a parameter.
                   // The AddRegionViewModel class contains the properties required to create a new region.
        [Route("Add")]
        public async Task<IActionResult> Add(AddRegionViewModelWithErrorMessage addRegionViewModel)
        {
            if (!ModelState.IsValid)
            {
                // ModelState is invalid, handle errors
                //foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    Console.WriteLine(error.ErrorMessage);
                //}
                return View(addRegionViewModel);          // By returning the addRegionViewModel with the View(),
                                                          // you're sending back the data that the user entered.
                                                          // This allows the form to be re - rendered with the
                                                          // user's input still present in the input fields.
            }
            try
            {
                //var client = await _httpClientAuthService.GetAuthorizedClient();
                ////var client = _httpClient.CreateClient(); // use IHttpClientFactory to create an instance of HttpClient
                //string token = GetJwtTokenFromCookies();
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                //}
                //else
                //{
                //    _logger.LogWarning("JWT Token was null or empty");
                //}
                addRegionViewModel.RegionViewModel.Code = addRegionViewModel.RegionViewModel.Code?.ToUpper(); // Convert to uppercase
                var response = await _regionApiService.AddRegionAsync(addRegionViewModel.RegionViewModel);

                if (response != null)
                {
                    return RedirectToAction("Index", "Regions");
                }
                addRegionViewModel.ErrorMessage = "The API did not return a valid RegionDto.";
                return View(addRegionViewModel);

                //var httpRequestMessage = new HttpRequestMessage()
                //{
                //    Method = HttpMethod.Post, // the HTTP method used for the request
                //    //RequestUri = new Uri("https://localhost:7152/regions"),
                //    // Configuration["Jwt:Issuer"]??
                //    RequestUri = new Uri($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/v1/regions"), // the URL of the resource to consume. ideally should be stored in the appsettings.json file
                //    Content = new StringContent(JsonSerializer.Serialize(addRegionViewModel.RegionViewModel), Encoding.UTF8, "application/json")
                //    // the content of the request. In this case, it's the addRegionViewModel object serialized to JSON
                //};

                //// Add the JWT token to the Authorization header
                //httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);


                //var httpResponseMessage = await client.SendAsync(httpRequestMessage);
                //_logger.LogInformation("Response Content: {responseContent}", httpResponseMessage.Content);

                //httpResponseMessage.EnsureSuccessStatusCode();

                //var response = await httpResponseMessage.Content.ReadFromJsonAsync<RegionDto>();
                //foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    Console.WriteLine(error.ErrorMessage);
                //}
                //if (response != null)
                //{
                //    return RedirectToAction("Index", "Regions");
                //}
                ////ViewBag.ErrorMessage = "The API did not return a valid RegionDto.";
                //addRegionViewModel.ErrorMessage = "The API did not return a valid RegionDto.";
                //return View(addRegionViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                //ViewBag.ErrorMessage = "An unexpected error occurred.";
                addRegionViewModel.ErrorMessage = "An unexpected error occurred.";
                return View(addRegionViewModel);
            }
        }

        /// <summary>
        /// This method handles HTTP GET requests to the /Edit endpoint.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Edit/{id:guid}")] // Matches /Regions/Edit/your-guid-here
        [Authorize(Roles = "Admin, Writer")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var region = await _regionApiService.GetRegionByIdAsync(id);
                if (region != null)
                {
                    return View(region);
                }
                return View(null);
                //var client = await _httpClientAuthService.GetAuthorizedClient();
                ////ViewBag.Id = id;
                ////var client = _httpClient.CreateClient(); // create a new HttpClient instance to use for consuming the web API
                //string token = GetJwtTokenFromCookies();
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                //}

                //var response = await client.GetFromJsonAsync<RegionDto>($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/v1/regions/{id.ToString()}"); // the URL of the resource to consume. ideally should be stored in the appsettings.json file
                //if (response != null)
                //{
                //    return View(response);
                //}
            }
            //catch (HttpRequestException ex)
            //{
            //    // Handle HTTP request errors (e.g., network issues, 404, 500)
            //    Console.WriteLine($"HTTP request error: {ex.Message}");
            //    // Log the exception details for debugging
            //    _logger.LogError(ex, "Error getting region.");
            //    // Consider returning a default value or throwing a custom exception
            //    return View(null); // or throw new CustomRegionException("Failed to get region.", ex);

            //}
            //catch (System.Text.Json.JsonException ex)
            //{
            //    // Handle JSON deserialization errors (e.g., invalid JSON format)
            //    Console.WriteLine($"JSON deserialization error: {ex.Message}");
            //    // Log the exception details for debugging.
            //    _logger.LogError(ex, "Error deserializing region JSON.");
            //    return View(null); // or throw new CustomRegionException("Failed to deserialize region data.", ex);
            //}
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Log the exception details for debugging
                _logger.LogError(ex, "An unexpected error occurred while getting region.");

                return View(null); //Or throw another custom exception.
            }
            //return View(null);
        }

        /// <summary>
        /// This method handles HTTP POST requests to the /Edit endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]

        [Route("Edit/{id:guid}")] // Matches /Regions/Edit/your-guid-here
        [Authorize(Roles = "Admin, Writer")]
        public async Task<IActionResult> Edit(UpdateRegionViewModel request)
        {
            try
            {
                //var client = await _httpClientAuthService.GetAuthorizedClient();
                //var client = _httpClient.CreateClient(); // create a new HttpClient instance to use for consuming the web API
                //string token = GetJwtTokenFromCookies();
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                //}

                //var httpRequestMessage = new HttpRequestMessage()
                //{
                //    Method = HttpMethod.Put,
                //    RequestUri = new Uri($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/v1/regions/{request.Id}"),
                //    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                //};
                //var httpResponseMessage = await client.SendAsync(httpRequestMessage); // the URL of the resource to consume. ideally should be stored in the appsettings.json file
                //httpResponseMessage.EnsureSuccessStatusCode();

                //var response = await httpResponseMessage.Content.ReadFromJsonAsync<RegionDto>();

                //if (response != null)
                //{
                //    return RedirectToAction("Index", "Regions"); // redirect to the Index action of the Regions controller
                //}
                //else
                //{
                //    _logger.LogWarning($"API returned null response for region ID '{request.Id}'.");
                //    TempData["ErrorMessage"] = "Failed to update region: API returned null.";
                //    return RedirectToAction("Edit", new { id = request.Id }); // TempData is designed for short-lived data that needs to persist only
                //                                                              // for the next request. It's particularly useful when you're redirecting
                //                                                              // from one action to another, as regular view data(ViewBag or ViewData)
                //                                                              // won't survive the redirect.
                //}
                var response = await _regionApiService.UpdateRegionAsync(request);
                if (response != null)
                {
                    return RedirectToAction("Index", "Regions");
                }
                TempData["ErrorMessage"] = "Failed to update region: API returned null.";
                return RedirectToAction("Edit", new { id = request.Id });
            }
            //catch (HttpRequestException ex)
            //{
            //    // Handle HTTP request errors (e.g., network issues, 404, 500)
            //    Console.WriteLine($"HTTP request error: {ex.Message}");
            //    // Log the exception details for debugging
            //    _logger.LogError(ex, "Error updating region.");
            //    TempData["ErrorMessage"] = "Failed to update region.";
            //    return RedirectToAction("Edit", new { id = request.Id });
            //}
            //catch (System.Text.Json.JsonException ex)
            //{
            //    // Handle JSON deserialization errors (e.g., invalid JSON format)
            //    Console.WriteLine($"Error deserializing region JSON: {ex.Message}");
            //    // Log the exception details for debugging.
            //    _logger.LogError(ex, "Error deserializing region JSON.");
            //    TempData["ErrorMessage"] = "Failed to deserialize region data.";
            //    return RedirectToAction("Edit", new { id = request.Id });
            //}
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Log the exception details for debugging
                _logger.LogError(ex, "An unexpected error occurred while updating region.");
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Edit", new { id = request.Id });
            }
            //TempData["ErrorMessage"] = "An unexpected error occurred."; // TempData is used because the error message needs to persist across a redirect
            //return RedirectToAction("Edit", new { id = request.Id });
        }

        /// <summary>
        /// This method handles HTTP POST requests to the /Delete endpoint.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost] //since the Delete method is invoked by a form submission, it should be decorated with the [HttpPost] attribute
        //public async Task<IActionResult> Delete(RegionDto request)
        [Route("Delete/{id:guid}")] // Matches /Regions/Delete/your-guid-here
        [Authorize(Roles = "Admin, Writer")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            try
            {
                await _regionApiService.DeleteRegionAsync(Id);
                //var client = await _httpClientAuthService.GetAuthorizedClient();
                ////var client = _httpClient.CreateClient(); // create a new HttpClient instance to use for consuming the web API

                //var token = GetJwtTokenFromCookies();
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                //}

                //var httpResponseMessage = await client.DeleteAsync($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/v1/regions/{request.Id}"); // the URL of the resource to consume. ideally should be stored in the appsettings.json file
                //httpResponseMessage.EnsureSuccessStatusCode();
                return RedirectToAction("Index", "Regions");
            }
            //catch (HttpRequestException ex)
            //{
            //    // Handle HTTP request errors (e.g., network issues, 404, 500)
            //    Console.WriteLine($"HTTP request error: {ex.Message}");
            //    // Log the exception details for debugging
            //    _logger.LogError(ex, "Error deleting region.");
            //    TempData["ErrorMessage"] = "Failed to delete region.";
            //    return RedirectToAction("Index", "Regions");
            //}
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred deleting region: {ex.Message}");
                // Log the exception details for debugging
                _logger.LogError(ex, "An unexpected error occurred while deleting region.");
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("Index", "Regions");
            }
        }
    }
}
