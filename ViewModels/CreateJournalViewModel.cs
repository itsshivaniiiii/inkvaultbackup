using System.ComponentModel.DataAnnotations;
using InkVault.Models;

namespace InkVault.ViewModels
{
    public class CreateJournalViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = null!;

        [StringLength(50)]
        public string? Topic { get; set; }

        public JournalStatus Status { get; set; } = JournalStatus.Draft;

        public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Private;
    }

    public class EditJournalViewModel
    {
        public int JournalId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = null!;

        [StringLength(50)]
        public string? Topic { get; set; }

        public JournalStatus Status { get; set; } = JournalStatus.Draft;

        public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Private;
    }

    public class JournalListViewModel
    {
        public int JournalId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public JournalStatus Status { get; set; }
        public PrivacyLevel PrivacyLevel { get; set; }
        public string? Topic { get; set; }
        public int ViewCount { get; set; }
    }
}
