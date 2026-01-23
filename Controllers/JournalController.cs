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
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JournalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateJournalViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var journal = new Journal
            {
                Title = model.Title,
                Content = model.Content,
                Topic = model.Topic,
                UserId = userId,
                Status = model.Status,
                PrivacyLevel = model.PrivacyLevel,
                CreatedAt = DateTime.UtcNow
            };

            _context.Journals.Add(journal);
            await _context.SaveChangesAsync();

            TempData["Success"] = model.Status == JournalStatus.Draft 
                ? "Journal saved as draft!" 
                : "Journal published successfully!";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.JournalId == id && j.UserId == userId);

            if (journal == null)
                return NotFound();

            var model = new EditJournalViewModel
            {
                JournalId = journal.JournalId,
                Title = journal.Title,
                Content = journal.Content,
                Topic = journal.Topic,
                Status = journal.Status,
                PrivacyLevel = journal.PrivacyLevel
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditJournalViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.JournalId == model.JournalId && j.UserId == userId);

            if (journal == null)
                return NotFound();

            journal.Title = model.Title;
            journal.Content = model.Content;
            journal.Topic = model.Topic;
            journal.Status = model.Status;
            journal.PrivacyLevel = model.PrivacyLevel;
            journal.UpdatedAt = DateTime.UtcNow;

            _context.Journals.Update(journal);
            await _context.SaveChangesAsync();

            TempData["Success"] = model.Status == JournalStatus.Draft 
                ? "Journal saved as draft!" 
                : "Journal updated successfully!";

            return RedirectToAction("MyJournals");
        }

        [HttpGet]
        public async Task<IActionResult> MyJournals()
        {
            var userId = _userManager.GetUserId(User);
            var journals = await _context.Journals
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new JournalListViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Content = j.Content.Length > 100 ? j.Content.Substring(0, 100) + "..." : j.Content,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,
                    Status = j.Status,
                    PrivacyLevel = j.PrivacyLevel,
                    Topic = j.Topic,
                    ViewCount = j.ViewCount
                })
                .ToListAsync();

            return View(journals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.JournalId == id && j.UserId == userId);

            if (journal == null)
                return NotFound();

            _context.Journals.Remove(journal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Journal deleted successfully!";
            return RedirectToAction("MyJournals");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> View(int id, bool fromBrowse = false, string? topicName = null, bool fromFeed = false)
        {
            var journal = await _context.Journals
                .Include(j => j.User)
                .Include(j => j.Views)
                .FirstOrDefaultAsync(j => j.JournalId == id);

            if (journal == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var userName = User?.Identity?.Name;
            
            System.Diagnostics.Debug.WriteLine($"\n=== JOURNAL VIEW DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Journal ID: {id}");
            System.Diagnostics.Debug.WriteLine($"User Name: {userName ?? "ANONYMOUS"}");
            System.Diagnostics.Debug.WriteLine($"User ID: {userId ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"Is Authenticated: {User?.Identity?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"========================\n");

            var isOwner = journal.UserId == userId;

            if (journal.Status != JournalStatus.Published && !isOwner)
                return Forbid();

            if (journal.PrivacyLevel == PrivacyLevel.Private && !isOwner)
                return Forbid();

            // Check if user is still friends with the author for friends-only journals
            if (journal.PrivacyLevel == PrivacyLevel.FriendsOnly && !isOwner && !string.IsNullOrEmpty(userId))
            {
                var isFriend = await _context.Friends
                    .AnyAsync(f => f.UserId == userId && f.FriendUserId == journal.UserId);

                if (!isFriend)
                    return Forbid();
            }

            // Track unique user view ONLY if user is logged in
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var existingView = await _context.JournalViews
                        .FirstOrDefaultAsync(v => v.JournalId == id && v.UserId == userId);

                    if (existingView == null)
                    {
                        // New view from this user - create record
                        var journalView = new JournalView
                        {
                            JournalId = id,
                            UserId = userId,
                            ViewedAt = DateTime.UtcNow
                        };

                        _context.JournalViews.Add(journalView);
                        await _context.SaveChangesAsync();
                        
                        // ONLY increment if INSERT succeeded
                        journal.ViewCount++;
                        _context.Journals.Update(journal);
                        await _context.SaveChangesAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"? NEW VIEW RECORDED: User={userId}, Journal={id}, NewCount={journal.ViewCount}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"??  DUPLICATE VIEW PREVENTED: User={userId}, Journal={id}, FirstViewedAt={existingView.ViewedAt}");
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") ?? false)
                {
                    // Handle race condition where another request already inserted the view
                    System.Diagnostics.Debug.WriteLine($"??  CONCURRENT VIEW PREVENTED (Unique Constraint): User={userId}, Journal={id}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? ERROR SAVING VIEW: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"??  ANONYMOUS VIEW: No view tracking (userId is null)");
            }

            // Pass browse/feed parameters to view
            ViewBag.FromBrowse = fromBrowse;
            ViewBag.TopicName = topicName;
            ViewBag.FromFeed = fromFeed;

            return View(journal);
        }
    }
}
