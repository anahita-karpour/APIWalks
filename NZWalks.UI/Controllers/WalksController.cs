using NZWalks.UI.Services.Auths;

namespace NZWalks.UI.Controllers
{
    public class WalksController : BaseController
    {
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IConfiguration _configuration;
        //private readonly ILogger<WalksController> _logger;
        //private readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        //private readonly IHttpClientAuthService _httpClientAuthService;
        //private readonly HttpClient _httpClient;

        //    public WalksController(
        //        IHttpClientFactory httpClientFactory,
        //        IConfiguration configuration,
        //        ILogger<WalksController> logger,
        //        IJwtTokenServiceUI jwtTokenServiceUI,
        //        IHttpClientAuthService httpClientAuthService), 
        //        HttpClient _httpClient)
        //    {
        //        _httpClientFactory = httpClientFactory;
        //        _configuration = configuration;
        //        _logger = logger;
        //        _jwtTokenServiceUI = jwtTokenServiceUI;
        //        _httpClientAuthService = httpClientAuthService;
        //        _httpClient = httpClient;
        //    }


        //    [HttpGet]
        //    public async Task<IActionResult> Index()
        //    {

        //        //List<WalkDto> walksResponse = new List<WalkDto>();
        //        //string baseUrl = _configuration.GetSection("ApiSettings")["BaseUrl"]; ; // get the base URL of the web API from the appsettings.json file

        //        //if (string.IsNullOrEmpty(baseUrl))
        //        //{
        //        //    _logger.LogError("BaseUrl is not configured");
        //        //    ViewBag.ErrorMessage = "The API base URL is not configured.";
        //        //    return View(new List<WalkDto>());
        //        //}
        //        try
        //        {
        //            var client = await _httpClientAuthService.GetAuthorizedClient();
        //            //var client = _httpClientFactory.CreateClient(); // create a new HttpClient instance to use for consuming the web API    
        //            // Get All walks from the web API
        //            // var client = _httpClient.CreateClient(); // create a new HttpClient instance to use for consuming the web API
        //            //Sending request to find web api REST service resource GetAllEmployees using HttpClient
        //            var httpResponseMessage =
        //                    await client.GetAsync($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/walks"); // the URL of the resource to consume. ideally should be stored in the appsettings.json file
        //                                                                                                           // the URL is found from lauch Settings.json and the Route of the AdminWalksController
        //            httpResponseMessage.EnsureSuccessStatusCode(); // throw an exception if the status code is not a success status code
        //            //var stringResponseBody = await httpResponseMessage.Content.ReadAsStringAsync(); // read the content of the response as a string
        //            var walksResponse = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<Models.DTO.WalkDto>>(); // read the content of the response as a JSON object
        //                                                                                                                        //using var content = await httpResponseMessage.Content.ReadAsStreamAsync(); // read the content of the response as a stream
        //                                                                                                                        //walksResponse.AddRange(await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<WalkDto>>()); // read the content of the response as a JSON object
        //                                                                                                                        //using var content = await httpResponseMessage.Content.ReadAsStreamAsync(); // read the content of the response as a stream

        //            //ViewBag.Response = stringResponseBody; // pass the response to the view
        //            return View(walksResponse);
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            // log the exception
        //            _logger.LogError(ex, "Error fetching walks from API.");
        //            ViewBag.ErrorMessage = "An error occured while fetching walks from API.";
        //            return View(new List<Models.DTO.WalkDto>()); // return an empty list of regions or an appropriate error message
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "An unexpected error occurred.");
        //            ViewBag.ErrorMessage = "An unexpected error occurred.";
        //            return View(new List<Models.DTO.WalkDto>());
        //        }
        //        return View();
        //    }

        //    //[Authorize(Roles = "Writer")]
        //    [HttpGet]
        //    public IActionResult Add()
        //    {
        //        // Create a new AddWalkViewModelWithErrorMessage and pass it to the view
        //        var viewModel = new AddWalkViewModelWithErrorMessage
        //        {
        //            WalkViewModel = new AddWalkViewModel() // Initialize AddWalkViewModel
        //            {
        //                LengthInKm = 0.0,
        //                RegionId = Guid.Empty,
        //                DifficultyId = Guid.Empty
        //            }
        //        };
        //        return View(viewModel);
        //    }

        //    [HttpGet]
        //    public async Task<IActionResult> Edit(Guid id)
        //    {
        //        //// Get the walk from the API
        //        //var client = await _httpClientAuthService.GetAuthorizedClient();
        //        //var httpResponseMessage = await client.GetAsync($"{_configuration.GetSection("ApiSettings")["BaseUrl"]}/walks/{id}");
        //        //httpResponseMessage.EnsureSuccessStatusCode();
        //        //var walkResponse = await httpResponseMessage.Content.ReadFromJsonAsync<WalkDto>();
        //        //// Map the walk to the view model
        //        //var viewModel = new AddWalkViewModelWithErrorMessage
        //        //{
        //        //    WalkViewModel = new AddWalkViewModel
        //        //    {
        //        //        Name = walkResponse.Name,
        //        //        Description = walkResponse.Description,
        //        //        LengthInKm = walkResponse.LengthInKm,
        //        //        WalkImageUrl = walkResponse.WalkImageUrl,
        //        //        RegionId = walkResponse.RegionId,
        //        //        DifficultyId = walkResponse.DifficultyId
        //        //    }
        //        //};
        //        //return View(viewModel);
        //        return View();
        //    }

        //    //[HttpPost]
        //    //public async Task<IActionResult> Edit(UpdateWalkRequestDto request)
        //    //{
        //    //    if (ModelState.IsValid)
        //    //    {
        //    //        // Logic to update the walk in the database using the request data
        //    //        var updatedWalk = await _walkRepository.UpdateAsync(request);

        //    //        if (updatedWalk != null)
        //    //        {
        //    //            // Redirect to a success page or the index
        //    //            return RedirectToAction("Index");
        //    //        }
        //    //        else
        //    //        {
        //    //            // Handle update failure
        //    //            ModelState.AddModelError("", "Failed to update the walk.");
        //    //            return View(request); // Return the view with errors
        //    //        }
        //    //    }

        //    //    // If ModelState is not valid, return the view with validation errors
        //    //    return View(request);
        //    //}

    }

}