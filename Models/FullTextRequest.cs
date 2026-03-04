using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkVault.Models
{
    /// <summary>
    /// Tracks requests from users to view full text of journals that only have abstracts
    /// </summary>
    public class FullTextRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int JournalId { get; set; }

        [ForeignKey("JournalId")]
        public Journal? Journal { get; set; }

        /// <summary>
        /// User who requested access to full text
        /// </summary>
        [Required]
        public string RequesterId { get; set; } = null!;

        [ForeignKey("RequesterId")]
        public ApplicationUser? Requester { get; set; }

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the journal owner has responded to this request
        /// </summary>
        public bool IsResponded { get; set; } = false;

        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Owner's response (granted/denied)
        /// </summary>
        public bool? IsGranted { get; set; }
    }
}
