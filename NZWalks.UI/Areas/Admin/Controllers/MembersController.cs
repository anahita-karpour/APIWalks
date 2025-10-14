using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.UI.Areas.Admin.Models.ViewModel;
using NZWalks.UI.Controllers;
using NZWalks.UI.Services.Members;

namespace NZWalks.UI.Areas.Admin.Controllers
{
    public class MembersController : BaseController
    {
        private readonly IMemberService _memberService;
        private readonly IMapper _mapper;

        public MembersController(
            IMemberService memberService,
            IMapper mapper
            )
        {
            _memberService = memberService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("Admin/Members/Index")]
        [Authorize(Roles = "Admin")] /*To restrict access to only users in the Admin role*/
        [Area("Admin")] /*To explicitly link the controller to the Admin area*/
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "User Management"; // Set the page title for the layout

            // Fetch users data from the database using the service
            var membersDto = await _memberService.GetAllMembersAsync();

            // Create the ViewModel
            var membersViewModel = new MembersViewModel();

            // Map the DTOs to MemberListItem for the list
            membersViewModel.Members = _mapper.Map<List<MemberListItem>>(membersDto);

            // Optionally, pre-fill the "Member Profile Management" section
            // with details of the *first* member in the list, or a specific one.
            if (membersDto != null && membersDto.Any())
            {
                var firstMember = membersDto.First();
                membersViewModel.UserId = firstMember.Id;
                membersViewModel.Email = firstMember.Email;
                membersViewModel.Role = firstMember.Role;
                // Password should remain null for security, as it's for input, not display
                membersViewModel.Password = null;
            }
            else
            {
                // Set default values if no members are found
                membersViewModel.UserId = Guid.Empty;
                membersViewModel.Email = "No members found.";
                membersViewModel.Password = null;
                membersViewModel.Role = "User";
            }

            return View(membersViewModel);
        }

        // You would add other actions here for Add, Edit, Delete later
        // public IActionResult Add() { ... }
        // public IActionResult Edit(Guid id) { ... }
        // public IActionResult Delete(Guid id) { ... }
    }
}
