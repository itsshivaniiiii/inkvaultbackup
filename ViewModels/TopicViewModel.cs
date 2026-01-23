using InkVault.Models;

namespace InkVault.ViewModels
{
    public class TopicCardViewModel
    {
        public string TopicName { get; set; } = null!;
        public int JournalCount { get; set; }
    }

    public class JournalTopicCardViewModel
    {
        public int JournalId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsOwn { get; set; }
        public PrivacyLevel PrivacyLevel { get; set; }
        public string AuthorId { get; set; } = null!;
    }

    public class JournalsByTopicViewModel
    {
        public string TopicName { get; set; } = null!;
        public List<JournalTopicCardViewModel> Journals { get; set; } = new List<JournalTopicCardViewModel>();
    }
}
