using System.ComponentModel.DataAnnotations;

namespace InkVault.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }
        public virtual Journal Journal { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
