using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using InkVault.Models;

namespace InkVault.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Journal> Journals { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<JournalView> JournalViews { get; set; }
        public DbSet<SavedJournal> SavedJournals { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<FullTextRequest> FullTextRequests { get; set; }

        // Data Protection Keys for antiforgery token encryption
        public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Friend relationships
            modelBuilder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany(u => u.FriendsInitiated)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany(u => u.FriendsReceived)
                .HasForeignKey(f => f.FriendUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure FriendRequest relationships
            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.FriendRequestsSent)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.FriendRequestsReceived)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure JournalView relationships
            modelBuilder.Entity<JournalView>()
                .HasOne(jv => jv.Journal)
                .WithMany(j => j.Views)
                .HasForeignKey(jv => jv.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalView>()
                .HasOne(jv => jv.User)
                .WithMany(u => u.JournalViews)
                .HasForeignKey(jv => jv.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Create unique index to prevent duplicate views from same user
            modelBuilder.Entity<JournalView>()
                .HasIndex(jv => new { jv.JournalId, jv.UserId })
                .IsUnique();

            // Configure FullTextRequest relationships
            modelBuilder.Entity<FullTextRequest>()
                .HasOne(ftr => ftr.Journal)
                .WithMany()
                .HasForeignKey(ftr => ftr.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FullTextRequest>()
                .HasOne(ftr => ftr.Requester)
                .WithMany()
                .HasForeignKey(ftr => ftr.RequesterId)
                .OnDelete(DeleteBehavior.NoAction);

            // Create unique index to prevent duplicate requests from same user for same journal
            modelBuilder.Entity<FullTextRequest>()
                .HasIndex(ftr => new { ftr.JournalId, ftr.RequesterId })
                .IsUnique();
        }
    }
}
