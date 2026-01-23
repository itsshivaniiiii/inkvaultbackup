using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkVault.Models
{
    public class Journal
    {
        [Key]
        public int JournalId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public JournalStatus Status { get; set; } = JournalStatus.Draft;

        [Required]
        public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Private;

        public string? Topic { get; set; }

        public int ViewCount { get; set; } = 0;

        // Navigation property for journal views
        public ICollection<JournalView> Views { get; set; } = new List<JournalView>();
    }

    public enum JournalStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum PrivacyLevel
    {
        Private = 0,
        FriendsOnly = 1,
        Public = 2
    }
}
