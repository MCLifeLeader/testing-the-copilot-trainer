using System.ComponentModel.DataAnnotations;

namespace MyChatApp.ApiService.Models
{
    /// <summary>
    /// User profile data transfer object
    /// </summary>
    public class UserProfileDto
    {
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public ProfileVisibilityDto ProfileVisibility { get; set; }
    }

    /// <summary>
    /// User profile update request
    /// </summary>
    public class UpdateUserProfileRequest
    {
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string? DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }

        public ProfileVisibilityDto? ProfileVisibility { get; set; }
    }

    /// <summary>
    /// Profile visibility options for API
    /// </summary>
    public enum ProfileVisibilityDto
    {
        Public = 0,
        ContactsOnly = 1,
        Private = 2
    }
}