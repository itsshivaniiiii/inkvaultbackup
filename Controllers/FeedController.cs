using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InkVault.Data;
using InkVault.Models;
using InkVault.ViewModels;

namespace InkVault.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // Get all friends of current user
            var friendIds = await _context.Friends
                .Where(f => f.UserId == currentUser.Id)
                .Select(f => f.FriendUserId)
                .ToListAsync();

            if (!friendIds.Any())
            {
                // No friends yet
                var emptyViewModel = new FriendsFeedViewModel
                {
                    PublicJournals = new List<FriendJournalViewModel>(),
                    FriendsOnlyJournals = new List<FriendJournalViewModel>(),
                    HasFriends = false
                };
                return View(emptyViewModel);
            }

            // Get public journals from friends
            var publicJournals = await _context.Journals
                .Where(j => j.Status == JournalStatus.Published &&
                           j.PrivacyLevel == PrivacyLevel.Public &&
                           friendIds.Contains(j.UserId))
                .Include(j => j.User)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // Get friends-only journals from friends
            var friendsOnlyJournals = await _context.Journals
                .Where(j => j.Status == JournalStatus.Published &&
                           j.PrivacyLevel == PrivacyLevel.FriendsOnly &&
                           friendIds.Contains(j.UserId))
                .Include(j => j.User)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // Helper method to strip HTML tags
            string StripHtmlTags(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;
                var regexRemoveHtmlTags = new System.Text.RegularExpressions.Regex("<[^>]*>");
                return regexRemoveHtmlTags.Replace(input, string.Empty).Trim();
            }

            var viewModel = new FriendsFeedViewModel
            {
                PublicJournals = publicJournals.Select(j => new FriendJournalViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Content = StripHtmlTags(j.Content).Length > 150
                        ? StripHtmlTags(j.Content).Substring(0, 150) + "..."
                        : StripHtmlTags(j.Content),
                    AuthorName = $"{j.User?.FirstName} {j.User?.LastName}",
                    AuthorProfilePicture = j.User?.ProfilePicturePath,
                    CreatedAt = j.CreatedAt,
                    ViewCount = j.ViewCount,
                    Topic = j.Topic,
                    PrivacyLevel = j.PrivacyLevel
                }).ToList(),

                FriendsOnlyJournals = friendsOnlyJournals.Select(j => new FriendJournalViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Content = StripHtmlTags(j.Content).Length > 150
                        ? StripHtmlTags(j.Content).Substring(0, 150) + "..."
                        : StripHtmlTags(j.Content),
                    AuthorName = $"{j.User?.FirstName} {j.User?.LastName}",
                    AuthorProfilePicture = j.User?.ProfilePicturePath,
                    CreatedAt = j.CreatedAt,
                    ViewCount = j.ViewCount,
                    Topic = j.Topic,
                    PrivacyLevel = j.PrivacyLevel
                }).ToList(),

                HasFriends = true
            };

            return View(viewModel);
        }
    }
}
