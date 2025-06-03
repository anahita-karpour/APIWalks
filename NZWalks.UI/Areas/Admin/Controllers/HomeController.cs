using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace NZWalks.UI.Areas.Admin.Controllers
{
    [Area("Admin")] // This attribute explicitly declares that this controller belongs to the "Admin" area.
    // This is crucial for the routing to work correctly.
    //[AllowAnonymous] // This attribute allows anonymous access to the controller, meaning that no authentication is required to access it.
    [Authorize(Roles = "Admin")] // Apply authorization to the entire controller
    //[Authorize(Policy = "AdminOnlyInAdminArea")]
    [Route("Admin/[controller]")]

    public class HomeController : Controller
    {
        // GET: Admin/Home
        // This action method handles GET requests to the "Admin/Home" route.
        // It returns the default view for the admin area.
        // This defines an action method named Index.
        // It's the default action that will be executed if no specific action is requested in the URL (e.g., /Admin or /Admin/Home).
        [HttpGet]
        //[Route("Admin/Home/Index")] // This attribute specifies the route for this action method.
        //In ASP.NET Core routing, 
        //he URL structure for an area is /[Area]/[Controller]/[Action]

        public IActionResult Index()
        {
            Debug.WriteLine("Admin/HomeController - Index action reached!");
            Console.WriteLine($"Admin: {User.IsInRole("Admin")}");

            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            return View();
        }
    }
}