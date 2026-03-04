using System.ComponentModel.DataAnnotations;

namespace InkVault.Models
{
    public class SavedJournal
    {
        public int SavedJournalId { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int JournalId { get; set; }
        public virtual Journal Journal { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}