using System.Diagnostics;
using InkVault.Models;
using InkVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using InkVault.Data;

namespace InkVault.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                
                // Check if this is the first login session
                // hasCompletedFirstLogin = false means this is the first time logging in ever
                bool isFirstLoginSession = !user.HasCompletedFirstLogin;
                ViewData["IsFirstLogin"] = isFirstLoginSession;

                // Update LastLoginAt
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                // Get published journals count (public + private)
                var publishedJournalsCount = await _context.Journals
                    .Where(j => j.UserId == userId && j.Status == JournalStatus.Published)
                    .CountAsync();

                // Get friends count
                try
                {
                    var friendsCount = await _context.Friends
                        .Where(f => f.UserId == userId)
                        .CountAsync();
                    ViewData["FriendsCount"] = friendsCount;
                }
                catch
                {
                    // Friends table doesn't exist yet - run Update-Database
                    ViewData["FriendsCount"] = 0;
                }

                // Get unique readers count (unique users who viewed any of the current user's journals)
                var readersCount = await _context.JournalViews
                    .Include(jv => jv.Journal)
                    .Where(jv => jv.Journal.UserId == userId)
                    .Select(jv => jv.UserId)
                    .Distinct()
                    .CountAsync();
                ViewData["ReadersCount"] = readersCount;

                // Get total likes count for all current user's journals
                try
                {
                    var likesCount = await _context.Likes
                        .Include(l => l.Journal)
                        .Where(l => l.Journal.UserId == userId)
                        .CountAsync();
                    ViewData["LikesCount"] = likesCount;
                }
                catch
                {
                    // Likes table doesn't exist yet - run Update-Database
                    ViewData["LikesCount"] = 0;
                }

                // Get total comments count for all current user's journals
                try
                {
                    var commentsCount = await _context.Comments
                        .Include(c => c.Journal)
                        .Where(c => c.Journal.UserId == userId)
                        .CountAsync();
                    ViewData["CommentsCount"] = commentsCount;
                }
                catch
                {
                    // Comments table doesn't exist yet - run Update-Database
                    ViewData["CommentsCount"] = 0;
                }

                ViewData["PublishedJournalsCount"] = publishedJournalsCount;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
