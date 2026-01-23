namespace InkVault.Models
{
    public class JournalView
    {
        public int JournalViewId { get; set; }
        public int JournalId { get; set; }
        public Journal Journal { get; set; } = null!;
        
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public DateTime ViewedAt { get; set; }
    }
}
