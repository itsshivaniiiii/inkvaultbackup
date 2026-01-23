namespace InkVault.ViewModels
{
    public class FriendsViewModel
    {
        public int FriendId { get; set; }
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FriendRequestViewModel
    {
        public int FriendRequestId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderFirstName { get; set; } = null!;
        public string SenderLastName { get; set; } = null!;
        public string? SenderProfilePicture { get; set; }
        public string? ReceiverId { get; set; }
        public string? ReceiverFirstName { get; set; }
        public string? ReceiverLastName { get; set; }
        public string? ReceiverProfilePicture { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FriendsManagementViewModel
    {
        public List<FriendsViewModel> Friends { get; set; } = new List<FriendsViewModel>();
        public List<FriendRequestViewModel> PendingRequests { get; set; } = new List<FriendRequestViewModel>();
        public List<FriendRequestViewModel> SentPendingRequests { get; set; } = new List<FriendRequestViewModel>();
        public List<UserSearchResultViewModel> SearchResults { get; set; } = new List<UserSearchResultViewModel>();
        public string? SearchQuery { get; set; }
        public int FriendCount { get; set; }
    }

    public class UserSearchResultViewModel
    {
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfilePicturePath { get; set; }
        public string? FriendStatus { get; set; } // "friends", "pending", "not-friends"
        public int? FriendId { get; set; } // For removing friends
        public int? FriendRequestId { get; set; } // For withdrawing requests
    }
}
