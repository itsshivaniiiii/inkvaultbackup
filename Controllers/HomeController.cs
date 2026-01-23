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

                ViewData["PublishedJournalsCount"] = publishedJournalsCount;
                ViewData["ReadersCount"] = 0; // Placeholder
                ViewData["LikesAndCommentsCount"] = 0; // Placeholder
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
