using System.ComponentModel.DataAnnotations;

namespace InkVault.Models
{
    public class Like
    {
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }
        public virtual Journal Journal { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
