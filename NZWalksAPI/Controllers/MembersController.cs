using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Text.Json;

namespace NZWalks.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")] // This sets the base route api/members for the controller
    [ApiController] // This attribute makes the controller an API controller
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class MembersController : ControllerBase
    {
        private readonly IMapper _autoMapper;
        private readonly ILogger<MembersController> _logger;
        private readonly IMemberRepository _memberRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MembersController(
            IMapper autoMapper,
            ILogger<MembersController> logger,
            IMemberRepository memberRepository,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _autoMapper = autoMapper;
            _logger = logger;
            _memberRepository = memberRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/V1/members
        /// <summary>
        /// Get all members (users) from the database, combining IdentityUser and custom Member profile data.
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMembers()
        {
            _logger.LogInformation("Attempting to get all members from database.");

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Console.WriteLine($"User is authenticated: {HttpContext.User.Identity.Name}");
                foreach (var claim in HttpContext.User.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }
            }
            else
            {
                _logger.LogInformation("User is NOT authenticated.");
            }
            try
            {
                // 1. Get all IdentityUsers (these are the core user accounts)
                var identityUsers = await _userManager.Users.ToListAsync();

                var membersDto = new List<MemberDto>();

                // 2. Iterate through each IdentityUser to get their roles and build the DTO
                foreach (var user in identityUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // 3. Optionally, fetch the custom Member profile if it exists
                    // This is where you include additional profile data if your Member class has it
                    var memberProfile = await _memberRepository.GetMemberByIdentityUserIdAsync(user.Id);

                    membersDto.Add(new MemberDto
                    {
                        Id = Guid.Parse(user.Id), // IdentityUser.Id is string, MemberDto.Id is Guid
                        Email = user.Email,
                        Role = string.Join(", ", roles), // Combine all roles into a single string
                        // Add other properties from memberProfile if they exist and are needed in MemberDto
                        // e.g., FirstName = memberProfile?.FirstName,
                        // LastName = memberProfile?.LastName
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} members.", membersDto.Count);
                return Ok(membersDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting all members: {Message}", e.Message);
                return StatusCode(500, "Internal Server Error during member retrieval."); // Return appropriate error
            }
        }

        /// <summary>
        /// Get a single member by ID from the database
        /// </summary>
        /// <param name="id">The ID of the member to retrieve (this is the IdentityUser's Guid ID)</param>
        [HttpGet]
        [Route("{id:Guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMemberById([FromRoute] Guid id)
        {
            _logger.LogInformation("Attempting to get member with ID: {Id} from database.", id);

            // 1. Find the IdentityUser first
            var identityUser = await _userManager.FindByIdAsync(id.ToString()); // UserManager takes string ID

            if (identityUser == null)
            {
                _logger.LogWarning("Member (IdentityUser) with ID: {Id} not found in database.", id);
                return NotFound();
            }

            // 2. Get the roles for this IdentityUser
            var roles = await _userManager.GetRolesAsync(identityUser);

            // 3. Fetch the custom Member profile linked to this IdentityUser
            var memberProfile = await _memberRepository.GetMemberByIdentityUserIdAsync(identityUser.Id);

            // 4. Construct the MemberDto
            var memberDto = new MemberDto
            {
                Id = Guid.Parse(identityUser.Id),
                Email = identityUser.Email,
                Role = string.Join(", ", roles) // Combine all roles into a single string
                // Add other properties from memberProfile if they exist and are needed in MemberDto
                // e.g., FirstName = memberProfile?.FirstName ?? "N/A",
            };
            _logger.LogInformation("Successfully retrieved member with ID: {Id}.", id);
            return Ok(memberDto);
        }


        /// <summary>
        /// Update a member's details (email and roles) by ID.
        /// </summary>
        /// <param name="id">The ID of the member (IdentityUser's Guid) to update.</param>
        /// <param name="request">The update request DTO containing new email and roles.</param>
        /// <returns>The updated member DTO.</returns>
        [HttpPut]
        [Route("{id:Guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")] // Only Admins can update member details
        public async Task<IActionResult> UpdateMemberAsync([FromRoute] Guid id, [FromBody] UpdateMemberRequestDto requestDto)
        {
            _logger.LogInformation("Attempting to update member with ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateMember request for ID {Id} failed due to validation errors: {Errors}", id, ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Find the IdentityUser first
                var identityUser = await _userManager.FindByIdAsync(id.ToString());
                if (identityUser == null)
                {
                    _logger.LogWarning("Member (IdentityUser) with ID: {Id} not found for update.", id);
                    return NotFound();
                }

                // 2. Fetch the custom Member profile linked to this IdentityUser
                var memberProfile = await _memberRepository.GetMemberByIdentityUserIdAsync(identityUser.Id);
                if (memberProfile == null)
                {
                    _logger.LogWarning("Custom Member profile for IdentityUser ID: {IdentityUserId} not found for update. Creating one.", identityUser.Id);
                    //// Optionally create a member profile if it doesn't exist for an IdentityUser
                    //memberProfile = new Member
                    //{
                    //    Id = Guid.NewGuid(), // Generate new ID for the Member profile
                    //    IdentityUserId = identityUser.Id,
                    //    Email = identityUser.Email, // Initial email from IdentityUser
                    //    Role = "" // No initial role, will be set below
                    //};
                    //memberProfile = await _memberRepository.CreateMemberAsync(memberProfile);
                }

                // 3. Update IdentityUser's Email if changed
                if (identityUser.Email != requestDto.Email)
                {
                    var setEmail = await _userManager.SetEmailAsync(identityUser, requestDto.Email);
                    if (!setEmail.Succeeded)
                    {
                        var errors = string.Join("; ", setEmail.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to update email for IdentityUser ID {Id}: {Errors}", id, errors);
                        return StatusCode(500, $"Failed to update email: {errors}");
                    }
                }


                // 4. Update IdentityUser's Roles
                // Get current roles (even if we only expect one for this API's purpose)
                var currentIdentityUserRole = await _userManager.GetRolesAsync(identityUser);
                var newRole = requestDto.Role;

                // Check if the new role is actually one of the valid roles in your system
                if (!await _roleManager.RoleExistsAsync(newRole))
                {
                    _logger.LogWarning("Attempted to assign non-existent role: {NewRole} to user ID: {Id}", newRole, id);
                    return BadRequest($"Role '{newRole}' does not exist.");
                }

                // Remove all current roles and add the new one
                if (currentIdentityUserRole.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(identityUser, currentIdentityUserRole);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to remove existing roles for IdentityUser ID {Id}: {Errors}", id, errors);
                        return StatusCode(500, $"Failed to remove existing roles: {errors}");
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(identityUser, newRole);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add new role '{NewRole}' for IdentityUser ID {Id}: {Errors}", newRole, id, errors);
                    return StatusCode(500, $"Failed to add role '{newRole}': {errors}");
                }

                // 5. Update the custom Member profile
                memberProfile.Email = requestDto.Email;
                memberProfile.Role = newRole; // Assign the single new role

                var updatedMemberProfile = await _memberRepository.UpdateMemberAsync(memberProfile.Id, memberProfile);
                if (updatedMemberProfile == null)
                {
                    // This should ideally not happen if memberProfile was found or created
                    _logger.LogError("Failed to update custom Member profile for IdentityUser ID {IdentityUserId}.", identityUser.Id);
                    return StatusCode(500, "Failed to update custom member profile.");
                }

                // 6. Construct and return the updated DTO
                var rolesAfterUpdate = await _userManager.GetRolesAsync(identityUser); // Get the absolute latest roles
                var memberDto = new MemberDto
                {
                    Id = Guid.Parse(identityUser.Id),
                    Email = identityUser.Email,
                    Role = rolesAfterUpdate.FirstOrDefault() ?? "" // Assuming only one role, take the first, or empty string
                };

                _logger.LogInformation("Successfully updated member with ID: {Id}.", id);
                return Ok(memberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member with ID: {Id}. Message: {Message}", id, ex.Message);
                return StatusCode(500, "Internal Server Error during member update.");
            }
        }

        //public async Task<IActionResult> DeleteMemberAsync()
        //{
        //    throw Exception();
        //}

    }
}
