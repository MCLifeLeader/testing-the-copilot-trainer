using System.ComponentModel.DataAnnotations;

namespace MyChatApp.ApiService.Models
{
    /// <summary>
    /// Contact data transfer object
    /// </summary>
    public class ContactDto
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public ContactStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsRequester { get; set; }
    }

    /// <summary>
    /// Contact request data transfer object
    /// </summary>
    public class ContactRequestDto
    {
        public int Id { get; set; }
        public required string RequesterId { get; set; }
        public string? RequesterDisplayName { get; set; }
        public string? RequesterUserName { get; set; }
        public string? RequesterAvatarUrl { get; set; }
        public required string ReceiverId { get; set; }
        public string? ReceiverDisplayName { get; set; }
        public string? ReceiverUserName { get; set; }
        public string? ReceiverAvatarUrl { get; set; }
        public ContactStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// User search result
    /// </summary>
    public class UserSearchResultDto
    {
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public ContactStatusDto? ContactStatus { get; set; }
        public int? ContactId { get; set; }
    }

    /// <summary>
    /// Contact search request
    /// </summary>
    public class ContactSearchRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Search query must be between 1 and 100 characters")]
        public required string Query { get; set; }

        [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50")]
        public int Limit { get; set; } = 20;
    }

    /// <summary>
    /// Send contact request
    /// </summary>
    public class SendContactRequestDto
    {
        [Required]
        public required string ReceiverId { get; set; }
    }

    /// <summary>
    /// Update contact request status
    /// </summary>
    public class UpdateContactRequestDto
    {
        [Required]
        public ContactStatusDto Status { get; set; }
    }

    /// <summary>
    /// Contact status for API
    /// </summary>
    public enum ContactStatusDto
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Blocked = 3
    }
}