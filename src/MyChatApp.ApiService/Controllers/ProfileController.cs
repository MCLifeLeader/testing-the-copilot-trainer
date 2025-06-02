using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChatApp.ApiService.Models;
using MyChatApp.Web.Data;

namespace MyChatApp.ApiService.Controllers
{
    /// <summary>
    /// API controller for user profile management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(UserManager<ApplicationUser> userManager, ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Get the current user's profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetMyProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var profile = new UserProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                ProfileVisibility = (ProfileVisibilityDto)user.ProfileVisibility
            };

            return Ok(profile);
        }

        /// <summary>
        /// Get a user's profile by ID (respects privacy settings)
        /// </summary>
        /// <param name="userId">User ID to get profile for</param>
        /// <returns>User profile information if accessible</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            var requestingUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(userId);

            if (targetUser is null)
            {
                return NotFound();
            }

            // Check privacy settings
            if (targetUser.ProfileVisibility == ProfileVisibility.Private && 
                (requestingUser is null || requestingUser.Id != targetUser.Id))
            {
                return Forbid();
            }

            // For ContactsOnly, we'd need to check if users are contacts
            // For now, treating it as public since contact relationship isn't implemented yet
            if (targetUser.ProfileVisibility == ProfileVisibility.ContactsOnly &&
                (requestingUser is null || requestingUser.Id != targetUser.Id))
            {
                // TODO: Check if users are contacts when contact system is implemented
                _logger.LogInformation("ContactsOnly privacy check skipped - contact system not implemented");
            }

            var profile = new UserProfileDto
            {
                UserId = targetUser.Id,
                UserName = targetUser.UserName,
                Email = targetUser.Email,
                DisplayName = targetUser.DisplayName,
                Bio = targetUser.Bio,
                AvatarUrl = targetUser.AvatarUrl,
                ProfileVisibility = (ProfileVisibilityDto)targetUser.ProfileVisibility
            };

            // Don't expose email for other users unless it's public profile
            if (requestingUser?.Id != targetUser.Id && targetUser.ProfileVisibility != ProfileVisibility.Public)
            {
                profile.Email = null;
            }

            return Ok(profile);
        }

        /// <summary>
        /// Update the current user's profile
        /// </summary>
        /// <param name="request">Profile update request</param>
        /// <returns>Updated profile information</returns>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> UpdateMyProfileAsync([FromBody] UpdateUserProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            // Update fields if provided
            if (request.DisplayName is not null)
            {
                user.DisplayName = request.DisplayName;
            }

            if (request.Bio is not null)
            {
                user.Bio = request.Bio;
            }

            if (request.ProfileVisibility.HasValue)
            {
                user.ProfileVisibility = (ProfileVisibility)request.ProfileVisibility.Value;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User {UserId} updated their profile", user.Id);

            var updatedProfile = new UserProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                ProfileVisibility = (ProfileVisibilityDto)user.ProfileVisibility
            };

            return Ok(updatedProfile);
        }
    }
}