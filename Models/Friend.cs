using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkVault.Models
{
    public class Friend
    {
        [Key]
        public int FriendId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string FriendUserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("FriendUserId")]
        public ApplicationUser? FriendUser { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
