using Microsoft.AspNetCore.Identity;

namespace MyChatApp.Web.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// User's display name shown to other users
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// User's biography/about section
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// URL to user's avatar image
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Profile visibility setting
        /// </summary>
        public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Public;
    }

    /// <summary>
    /// Defines who can view the user's profile
    /// </summary>
    public enum ProfileVisibility
    {
        /// <summary>
        /// Profile is visible to everyone
        /// </summary>
        Public = 0,
        
        /// <summary>
        /// Profile is only visible to contacts/friends
        /// </summary>
        ContactsOnly = 1,
        
        /// <summary>
        /// Profile is private and only visible to the user
        /// </summary>
        Private = 2
    }
}