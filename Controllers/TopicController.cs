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
    public class TopicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TopicController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get all unique topics from published journals
            var topics = await _context.Journals
                .Where(j => j.Status == JournalStatus.Published && !string.IsNullOrEmpty(j.Topic))
                .Select(j => j.Topic)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var topicCards = topics.Select(topic => new TopicCardViewModel
            {
                TopicName = topic,
                JournalCount = _context.Journals
                    .Count(j => j.Status == JournalStatus.Published && 
                               j.Topic == topic &&
                               (j.PrivacyLevel == PrivacyLevel.Public || 
                                j.PrivacyLevel == PrivacyLevel.FriendsOnly))
            }).ToList();

            return View(topicCards);
        }

        [HttpGet]
        public async Task<IActionResult> ViewByTopic(string topicName)
        {
            if (string.IsNullOrEmpty(topicName))
                return RedirectToAction("Index");

            var currentUserId = _userManager.GetUserId(User);

            // Get all journals with this topic
            var journals = await _context.Journals
                .Where(j => j.Status == JournalStatus.Published && 
                           j.Topic == topicName &&
                           (j.PrivacyLevel == PrivacyLevel.Public || 
                            j.PrivacyLevel == PrivacyLevel.FriendsOnly))
                .Include(j => j.User)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // Check if user has friends who posted to this topic
            var userFriends = await _context.Friends
                .Where(f => f.UserId == currentUserId)
                .Select(f => f.FriendUserId)
                .ToListAsync();

            // Helper method to strip HTML tags
            string StripHtmlTags(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;
                var regexRemoveHtmlTags = new System.Text.RegularExpressions.Regex("<[^>]*>");
                return regexRemoveHtmlTags.Replace(input, string.Empty).Trim();
            }

            var viewModel = new JournalsByTopicViewModel
            {
                TopicName = topicName,
                Journals = journals.Select(j => new JournalTopicCardViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Content = StripHtmlTags(j.Content).Length > 200 
                        ? StripHtmlTags(j.Content).Substring(0, 200) + "..." 
                        : StripHtmlTags(j.Content),
                    AuthorName = $"{j.User?.FirstName} {j.User?.LastName}",
                    CreatedAt = j.CreatedAt,
                    ViewCount = j.ViewCount,
                    IsOwn = j.UserId == currentUserId,
                    PrivacyLevel = j.PrivacyLevel,
                    AuthorId = j.UserId
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
