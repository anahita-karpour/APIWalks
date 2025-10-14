using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Models.DTO;
using NZWalks.UI.Models.ViewModel;
using NZWalks.UI.Services.Auths;
using NZWalks.UI.Services.Regions;

namespace NZWalks.UI.Controllers
{
    [Route("[controller]")]
    public class RegionsController : BaseController
    {
        private readonly IRegionService _regionService;
        private readonly ILogger<RegionsController> _logger;
        private readonly IJwtTokenServiceUI _jwtTokenServiceUI;
        private readonly IMapper _mapper;

        /// <summary>
        /// This constructor initializes the RegionsController with the required services.
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public RegionsController(
            IRegionService regionService,
            ILogger<RegionsController> logger,
            IJwtTokenServiceUI jwtTokenServiceUI,
            IMapper mapper)
        {
            _regionService = regionService;
            _logger = logger;
            _jwtTokenServiceUI = jwtTokenServiceUI;
            _mapper = mapper;
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
                var regions = await _regionService.GetAllRegionsAsync();
                if (regions != null)
                {
                    return View(regions);
                }
                return View(new List<RegionDtoV1>()); // Return an empty list if regions is null.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                return View(new List<RegionDtoV1>());
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
                return View(addRegionViewModel);          // By returning the addRegionViewModel with the View(),
                                                          // you're sending back the data that the user entered.
                                                          // This allows the form to be re - rendered with the
                                                          // user's input still present in the input fields.
            }
            try
            {
                // Convert ViewModel to Request DTO before sending to the service
                addRegionViewModel.RegionViewModel.Code = addRegionViewModel.RegionViewModel.Code?.ToUpper(); // Convert to uppercase
                var addRegionRequestDto = _mapper.Map<AddRegionRequestDto>(addRegionViewModel.RegionViewModel);


                var response = await _regionService.AddRegionAsync(addRegionRequestDto); // Pass the Dto

                if (response != null)
                {
                    return RedirectToAction("Index", "Regions");
                }
                addRegionViewModel.ErrorMessage = "The API did not return a valid RegionDto.";
                return View(addRegionViewModel);
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
                var regionDto = await _regionService.GetRegionByIdAsync(id);
                if (regionDto != null)
                {
                    var updateRegionViewModel = _mapper.Map<UpdateRegionViewModel>(regionDto);

                    return View(updateRegionViewModel);
                }
                TempData["ErrorMessage"] = "Region not found.";
                return RedirectToAction("Index", "Regions");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Log the exception details for debugging
                _logger.LogError(ex, "An unexpected error occurred while getting region.");

                return View(null); //Or throw another custom exception.
            }
        }

        /// <summary>
        /// This method handles HTTP POST requests to the /Edit endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("Edit/{id:guid}")] // Matches /Regions/Edit/your-guid-here
        [Authorize(Roles = "Admin, Writer")]
        public async Task<IActionResult> Edit(UpdateRegionViewModel updateRequest)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return the ViewModel to the view so errors can be displayed
                return View(updateRequest);
            }

            // Map the ViewModel to the API Request DTO
            var updateRequestDto = _mapper.Map<UpdateRegionRequestDto>(updateRequest);

            try
            {
                var success = await _regionService.UpdateRegionAsync(updateRequestDto);

                if (success)
                {
                    TempData["SuccessMessage"] = "Region updated successfully.";
                    return RedirectToAction("Index", "Regions");
                }
                else // If 'success' is false (API call failed)
                {
                    TempData["ErrorMessage"] = "Failed to update region. Please ensure all fields are valid or check server logs.";
                    // Stay on the edit page to show the error
                    return View(updateRequest);
                }
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                _logger.LogError(ex, "An unexpected error occurred while updating region with ID {RegionId}.", updateRequest.Id);
                TempData["ErrorMessage"] = "An unexpected error occurred while processing your request.";
                // Return to the edit page with the error
                return View(updateRequest);
            }
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
                await _regionService.DeleteRegionAsync(Id);
                return RedirectToAction("Index", "Regions");
            }
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