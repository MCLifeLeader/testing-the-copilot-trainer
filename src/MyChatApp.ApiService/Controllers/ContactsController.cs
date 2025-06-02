using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChatApp.ApiService.Models;
using MyChatApp.Web.Data;

namespace MyChatApp.ApiService.Controllers
{
    /// <summary>
    /// API controller for contact management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ContactsController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Search for users by display name, username, or email
        /// </summary>
        /// <param name="request">Search parameters</param>
        /// <returns>List of matching users with contact status</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<UserSearchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UserSearchResultDto>>> SearchUsersAsync([FromBody] ContactSearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var query = request.Query.ToLower().Trim();

            // Search users by display name, username, or email with fuzzy matching
            var users = await _userManager.Users
                .Where(u => u.Id != currentUser.Id &&
                           (u.DisplayName != null && u.DisplayName.ToLower().Contains(query) ||
                            u.UserName != null && u.UserName.ToLower().Contains(query) ||
                            u.Email != null && u.Email.ToLower().Contains(query)))
                .Take(request.Limit)
                .ToListAsync();

            // Get existing contact relationships
            var userIds = users.Select(u => u.Id).ToList();
            var contacts = await _context.Contacts
                .Where(c => (c.RequesterId == currentUser.Id && userIds.Contains(c.ReceiverId)) ||
                           (c.ReceiverId == currentUser.Id && userIds.Contains(c.RequesterId)))
                .ToListAsync();

            var results = users.Select(user =>
            {
                var contact = contacts.FirstOrDefault(c =>
                    (c.RequesterId == currentUser.Id && c.ReceiverId == user.Id) ||
                    (c.ReceiverId == currentUser.Id && c.RequesterId == user.Id));

                return new UserSearchResultDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    ContactStatus = contact?.Status switch
                    {
                        ContactStatus.Pending => ContactStatusDto.Pending,
                        ContactStatus.Accepted => ContactStatusDto.Accepted,
                        ContactStatus.Rejected => ContactStatusDto.Rejected,
                        ContactStatus.Blocked => ContactStatusDto.Blocked,
                        _ => null
                    },
                    ContactId = contact?.Id
                };
            }).ToList();

            return Ok(results);
        }

        /// <summary>
        /// Get all contacts for the current user
        /// </summary>
        /// <returns>List of contacts grouped by status</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContactDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetContactsAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var contacts = await _context.Contacts
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .Where(c => c.RequesterId == currentUser.Id || c.ReceiverId == currentUser.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var contactDtos = contacts.Select(contact =>
            {
                var isRequester = contact.RequesterId == currentUser.Id;
                var otherUser = isRequester ? contact.Receiver : contact.Requester;

                return new ContactDto
                {
                    Id = contact.Id,
                    UserId = otherUser.Id,
                    UserName = otherUser.UserName,
                    DisplayName = otherUser.DisplayName,
                    Email = otherUser.Email,
                    AvatarUrl = otherUser.AvatarUrl,
                    Status = (ContactStatusDto)contact.Status,
                    CreatedAt = contact.CreatedAt,
                    UpdatedAt = contact.UpdatedAt,
                    IsRequester = isRequester
                };
            }).ToList();

            return Ok(contactDtos);
        }

        /// <summary>
        /// Send a contact request to another user
        /// </summary>
        /// <param name="request">Contact request details</param>
        /// <returns>Created contact request</returns>
        [HttpPost("requests")]
        [ProducesResponseType(typeof(ContactRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ContactRequestDto>> SendContactRequestAsync([FromBody] SendContactRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var targetUser = await _userManager.FindByIdAsync(request.ReceiverId);
            if (targetUser is null)
            {
                return NotFound("User not found");
            }

            if (currentUser.Id == targetUser.Id)
            {
                return BadRequest("Cannot send contact request to yourself");
            }

            // Check if contact relationship already exists
            var existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c =>
                    (c.RequesterId == currentUser.Id && c.ReceiverId == targetUser.Id) ||
                    (c.RequesterId == targetUser.Id && c.ReceiverId == currentUser.Id));

            if (existingContact is not null)
            {
                return Conflict("Contact relationship already exists");
            }

            var contact = new Contact
            {
                RequesterId = currentUser.Id,
                ReceiverId = targetUser.Id,
                Status = ContactStatus.Pending
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            // Reload with user data
            contact = await _context.Contacts
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .FirstAsync(c => c.Id == contact.Id);

            var dto = new ContactRequestDto
            {
                Id = contact.Id,
                RequesterId = contact.RequesterId,
                RequesterDisplayName = contact.Requester.DisplayName,
                RequesterUserName = contact.Requester.UserName,
                RequesterAvatarUrl = contact.Requester.AvatarUrl,
                ReceiverId = contact.ReceiverId,
                ReceiverDisplayName = contact.Receiver.DisplayName,
                ReceiverUserName = contact.Receiver.UserName,
                ReceiverAvatarUrl = contact.Receiver.AvatarUrl,
                Status = (ContactStatusDto)contact.Status,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };

            return CreatedAtAction(nameof(GetContactAsync), new { id = contact.Id }, dto);
        }

        /// <summary>
        /// Get a specific contact by ID
        /// </summary>
        /// <param name="id">Contact ID</param>
        /// <returns>Contact details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContactRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ContactRequestDto>> GetContactAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var contact = await _context.Contacts
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact is null)
            {
                return NotFound();
            }

            // Check if the current user is involved in this contact
            if (contact.RequesterId != currentUser.Id && contact.ReceiverId != currentUser.Id)
            {
                return Forbid();
            }

            var dto = new ContactRequestDto
            {
                Id = contact.Id,
                RequesterId = contact.RequesterId,
                RequesterDisplayName = contact.Requester.DisplayName,
                RequesterUserName = contact.Requester.UserName,
                RequesterAvatarUrl = contact.Requester.AvatarUrl,
                ReceiverId = contact.ReceiverId,
                ReceiverDisplayName = contact.Receiver.DisplayName,
                ReceiverUserName = contact.Receiver.UserName,
                ReceiverAvatarUrl = contact.Receiver.AvatarUrl,
                Status = (ContactStatusDto)contact.Status,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Update a contact request status (accept, reject, block)
        /// </summary>
        /// <param name="id">Contact ID</param>
        /// <param name="request">Status update</param>
        /// <returns>Updated contact</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ContactRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ContactRequestDto>> UpdateContactAsync(int id, [FromBody] UpdateContactRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var contact = await _context.Contacts
                .Include(c => c.Requester)
                .Include(c => c.Receiver)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact is null)
            {
                return NotFound();
            }

            // Check if the current user is involved in this contact
            if (contact.RequesterId != currentUser.Id && contact.ReceiverId != currentUser.Id)
            {
                return Forbid();
            }

            // Validation rules for status updates
            if (request.Status == ContactStatusDto.Pending)
            {
                return BadRequest("Cannot set status back to pending");
            }

            // Only the receiver can accept/reject pending requests
            if (contact.Status == ContactStatus.Pending && 
                (request.Status == ContactStatusDto.Accepted || request.Status == ContactStatusDto.Rejected) &&
                contact.ReceiverId != currentUser.Id)
            {
                return Forbid("Only the receiver can accept or reject pending requests");
            }

            contact.Status = (ContactStatus)request.Status;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = new ContactRequestDto
            {
                Id = contact.Id,
                RequesterId = contact.RequesterId,
                RequesterDisplayName = contact.Requester.DisplayName,
                RequesterUserName = contact.Requester.UserName,
                RequesterAvatarUrl = contact.Requester.AvatarUrl,
                ReceiverId = contact.ReceiverId,
                ReceiverDisplayName = contact.Receiver.DisplayName,
                ReceiverUserName = contact.Receiver.UserName,
                ReceiverAvatarUrl = contact.Receiver.AvatarUrl,
                Status = (ContactStatusDto)contact.Status,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Delete a contact relationship
        /// </summary>
        /// <param name="id">Contact ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteContactAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact is null)
            {
                return NotFound();
            }

            // Check if the current user is involved in this contact
            if (contact.RequesterId != currentUser.Id && contact.ReceiverId != currentUser.Id)
            {
                return Forbid();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}