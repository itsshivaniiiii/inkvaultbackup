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
    public class SavedJournalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavedJournalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("save/{journalId}")]
        public async Task<IActionResult> SaveJournal(int journalId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }
                
                // Check if journal exists and is published
                var journal = await _context.Journals
                    .Include(j => j.User)
                    .FirstOrDefaultAsync(j => j.JournalId == journalId && j.Status == JournalStatus.Published);

                if (journal == null)
                    return NotFound(new { message = "Journal not found or not published" });

                // Check if user is trying to save their own journal
                if (journal.UserId == userId)
                    return BadRequest(new { message = "Cannot save your own journal" });

                // Check if already saved
                var existingSave = await _context.SavedJournals
                    .FirstOrDefaultAsync(s => s.JournalId == journalId && s.UserId == userId);

                if (existingSave != null)
                    return BadRequest(new { message = "Journal already saved" });

                // Save the journal
                var savedJournal = new SavedJournal
                {
                    JournalId = journalId,
                    UserId = userId,
                    SavedAt = DateTime.UtcNow
                };

                _context.SavedJournals.Add(savedJournal);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Journal saved successfully",
                    journalTitle = journal.Title,
                    authorName = $"{journal.User?.FirstName} {journal.User?.LastName}",
                    savedAt = savedJournal.SavedAt
                });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error saving journal: {ex.Message}");
                return StatusCode(500, new { message = "Failed to save journal", error = ex.Message });
            }
        }

        [HttpDelete("unsave/{journalId}")]
        public async Task<IActionResult> UnsaveJournal(int journalId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var savedJournal = await _context.SavedJournals
                    .Include(s => s.Journal)
                    .FirstOrDefaultAsync(s => s.JournalId == journalId && s.UserId == userId);

                if (savedJournal == null)
                    return NotFound(new { message = "Saved journal not found" });

                _context.SavedJournals.Remove(savedJournal);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Journal removed from saved list",
                    journalTitle = savedJournal.Journal?.Title
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unsaving journal: {ex.Message}");
                return StatusCode(500, new { message = "Failed to remove journal", error = ex.Message });
            }
        }

        [HttpGet("check/{journalId}")]
        public async Task<IActionResult> CheckSaved(int journalId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { isSaved = false });
                }

                var isSaved = await _context.SavedJournals
                    .AnyAsync(s => s.JournalId == journalId && s.UserId == userId);

                return Ok(new { isSaved = isSaved });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking saved status: {ex.Message}");
                return Ok(new { isSaved = false });
            }
        }
    }
}