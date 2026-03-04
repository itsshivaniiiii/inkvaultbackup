using InkVault.Data;
using InkVault.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InkVault.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionController : ControllerBase
    {
        private readonly InkVault.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InteractionController(InkVault.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("like/{journalId}")]
        public async Task<IActionResult> Like(int journalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var journal = await _context.Journals
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                return NotFound(new { message = "Journal not found" });

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.JournalId == journalId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { liked = false, likeCount = await _context.Likes.CountAsync(l => l.JournalId == journalId) });
            }

            var like = new Like
            {
                JournalId = journalId,
                UserId = userId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.JournalId == journalId);
            return Ok(new { liked = true, likeCount = likeCount, journalTitle = journal.Title, authorName = $"{journal.User?.FirstName} {journal.User?.LastName}" });
        }

        [HttpPost("comment")]
        public async Task<IActionResult> Comment([FromBody] CommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var journal = await _context.Journals.FirstOrDefaultAsync(j => j.JournalId == model.JournalId);

            if (journal == null)
                return NotFound(new { message = "Journal not found" });

            var comment = new Comment
            {
                JournalId = model.JournalId,
                UserId = userId,
                Content = model.Content
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = comment.Id,
                content = comment.Content,
                author = user.UserName,
                authorName = $"{user.FirstName} {user.LastName}",
                createdAt = comment.CreatedAt.ToString("MMM dd, yyyy"),
                userId = userId,
                journalTitle = journal.Title
            });
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                .Include(c => c.Journal)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully", journalTitle = comment.Journal?.Title });
        }

        [HttpGet("likes/{journalId}")]
        public async Task<IActionResult> GetLikeCount(int journalId)
        {
            var likeCount = await _context.Likes
                .Where(l => l.JournalId == journalId)
                .CountAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userLiked = await _context.Likes
                .AnyAsync(l => l.JournalId == journalId && l.UserId == userId);

            return Ok(new { likeCount = likeCount, userLiked = userLiked });
        }
    }

    public class CommentViewModel
    {
        public int JournalId { get; set; }
        public string Content { get; set; }
    }
}
