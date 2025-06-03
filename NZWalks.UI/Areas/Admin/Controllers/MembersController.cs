using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NZWalks.UI.Areas.Admin.Controllers
{
    public class MembersController : Controller
    {
        [HttpGet]
        [Route("Admin/Members/Index")]
        [Authorize(Roles = "Admin")] /*To restrict access to only users in the Admin role*/
        [Area("Admin")] /*To explicitly link the controller to the Admin area*/
        public IActionResult Index()
        {
            ViewData["Title"] = "User Management"; // Set the page title for the layout
            // In a real application, you would fetch a list of users here
            // and pass them to the view. For now, it's just a placeholder.
            return View();
        }

        // You would add other actions here for Add, Edit, Delete later
        // public IActionResult Add() { ... }
        // public IActionResult Edit(Guid id) { ... }
        // public IActionResult Delete(Guid id) { ... }
    }
}
