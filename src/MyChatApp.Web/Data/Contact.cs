using System.ComponentModel.DataAnnotations;

namespace MyChatApp.Web.Data
{
    /// <summary>
    /// Represents a contact relationship between two users
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Unique identifier for the contact relationship
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the user who initiated the contact request
        /// </summary>
        [Required]
        public required string RequesterId { get; set; }

        /// <summary>
        /// User who initiated the contact request
        /// </summary>
        public ApplicationUser Requester { get; set; } = null!;

        /// <summary>
        /// ID of the user who received the contact request
        /// </summary>
        [Required]
        public required string ReceiverId { get; set; }

        /// <summary>
        /// User who received the contact request
        /// </summary>
        public ApplicationUser Receiver { get; set; } = null!;

        /// <summary>
        /// Current status of the contact relationship
        /// </summary>
        public ContactStatus Status { get; set; } = ContactStatus.Pending;

        /// <summary>
        /// When the contact request was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the contact status was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Status of a contact relationship
    /// </summary>
    public enum ContactStatus
    {
        /// <summary>
        /// Contact request is pending approval
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Contact request has been accepted
        /// </summary>
        Accepted = 1,

        /// <summary>
        /// Contact request has been rejected
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// User has been blocked
        /// </summary>
        Blocked = 3
    }
}