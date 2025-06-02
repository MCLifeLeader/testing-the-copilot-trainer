using MyChatApp.Web.Data;

namespace MyChatApp.Web.Models
{
    // Add properties to this class and update the server and client AuthenticationStateProviders
    // to expose more information about the authenticated user to the client.
    public class UserInfo
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public ProfileVisibility ProfileVisibility { get; set; }
    }
}