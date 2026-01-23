using InkVault.Models;

namespace InkVault.ViewModels
{
    public class FriendJournalViewModel
    {
        public int JournalId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string? AuthorProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public string? Topic { get; set; }
        public PrivacyLevel PrivacyLevel { get; set; }
    }

    public class FriendsFeedViewModel
    {
        public List<FriendJournalViewModel> PublicJournals { get; set; } = new List<FriendJournalViewModel>();
        public List<FriendJournalViewModel> FriendsOnlyJournals { get; set; } = new List<FriendJournalViewModel>();
        public bool HasFriends { get; set; }
    }
}
