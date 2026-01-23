using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkVault.Models
{
    public class FriendRequest
    {
        [Key]
        public int FriendRequestId { get; set; }

        [Required]
        public string SenderId { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = null!;

        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser? Receiver { get; set; }

        [Required]
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }
    }

    public enum FriendRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2
    }
}
