namespace InkVault.ViewModels
{
    public class SavedJournalListViewModel
    {
        public int SavedJournalId { get; set; }
        public int JournalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime SavedAt { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public string? DUI { get; set; }
        public string? ReferencedDUI { get; set; }
    }
}