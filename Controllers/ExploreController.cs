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
    public class ExploreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExploreController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? sortBy = "recent")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var query = _context.Journals
                .Where(j => j.Status == JournalStatus.Published)
                .Include(j => j.User)
                .AsQueryable();

            // Get current user's friend IDs
            var friendIds = await _context.Friends
                .Where(f => f.UserId == currentUser.Id)
                .Select(f => f.FriendUserId)
                .ToListAsync();

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(j => 
                    j.Title.ToLower().Contains(searchLower) ||
                    j.Content.ToLower().Contains(searchLower) ||
                    j.Topic!.ToLower().Contains(searchLower) ||
                    (j.User!.FirstName + " " + j.User.LastName).ToLower().Contains(searchLower)
                );
            }

            // Separate public journals only
            var allJournals = await query.ToListAsync();

            var publicJournals = allJournals
                .Where(j => j.PrivacyLevel == PrivacyLevel.Public)
                .ToList();

            // Sort journals
            switch (sortBy)
            {
                case "oldest":
                    publicJournals = publicJournals.OrderBy(j => j.CreatedAt).ToList();
                    break;
                case "views":
                    publicJournals = publicJournals.OrderByDescending(j => j.ViewCount).ToList();
                    break;
                case "recent":
                default:
                    publicJournals = publicJournals.OrderByDescending(j => j.CreatedAt).ToList();
                    break;
            }

            // Helper method to strip HTML tags
            string StripHtmlTags(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;
                var regexRemoveHtmlTags = new System.Text.RegularExpressions.Regex("<[^>]*>");
                return regexRemoveHtmlTags.Replace(input, string.Empty).Trim();
            }

            var viewModel = new ExploreListViewModel
            {
                PublicJournals = publicJournals.Select(j => new ExploreViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Content = j.Content,
                    UserId = j.UserId,
                    AuthorName = $"{j.User!.FirstName} {j.User.LastName}",
                    AuthorProfilePicture = j.User.ProfilePicturePath,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,
                    ViewCount = j.ViewCount,
                    Topic = j.Topic,
                    PreviewText = StripHtmlTags(j.Content).Length > 150 
                        ? StripHtmlTags(j.Content).Substring(0, 150) + "..." 
                        : StripHtmlTags(j.Content),
                    IsFriendsOnly = false,
                    IsPublic = true,
                    IsOwn = j.UserId == currentUser.Id
                }).ToList(),
                FriendJournals = new List<ExploreViewModel>(),
                SearchQuery = search,
                SortBy = sortBy
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var journal = await _context.Journals
                .Include(j => j.User)
                .Include(j => j.Views)
                .FirstOrDefaultAsync(j => j.JournalId == id && j.Status == JournalStatus.Published);

            if (journal == null)
                return NotFound();

            // Check access permissions
            if (journal.PrivacyLevel == PrivacyLevel.Private)
                return Forbid();

            if (journal.PrivacyLevel == PrivacyLevel.FriendsOnly)
            {
                var isFriend = await _context.Friends
                    .AnyAsync(f => f.UserId == currentUser.Id && f.FriendUserId == journal.UserId);

                if (!isFriend && journal.UserId != currentUser.Id)
                    return Forbid();
            }

            // Track unique user view
            try
            {
                var existingView = await _context.JournalViews
                    .FirstOrDefaultAsync(v => v.JournalId == id && v.UserId == currentUser.Id);

                if (existingView == null)
                {
                    // New view from this user - create record
                    var journalView = new JournalView
                    {
                        JournalId = id,
                        UserId = currentUser.Id,
                        ViewedAt = DateTime.UtcNow
                    };

                    _context.JournalViews.Add(journalView);
                    await _context.SaveChangesAsync();
                    
                    // ONLY increment if INSERT succeeded
                    journal.ViewCount++;
                    _context.Journals.Update(journal);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") ?? false)
            {
                // Handle race condition where another request already inserted the view
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving view: {ex.Message}");
            }

            return View(journal);
        }
    }
}
