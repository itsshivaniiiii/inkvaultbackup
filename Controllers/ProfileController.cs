using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InkVault.Models;
using InkVault.ViewModels;

namespace InkVault.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.UserName,
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                ProfilePictureUrl = user.ProfilePicturePath,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                ThemePreference = user.ThemePreference
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model, IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(model?.FirstName) || string.IsNullOrWhiteSpace(model?.LastName))
            {
                ModelState.AddModelError(string.Empty, "First name and last name are required.");
                model = new ProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Username = user.UserName,
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    ProfilePictureUrl = user.ProfilePicturePath,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    ThemePreference = user.ThemePreference
                };
                return View("Index", model);
            }

            try
            {
                user.FirstName = model.FirstName?.Trim() ?? user.FirstName;
                user.LastName = model.LastName?.Trim() ?? user.LastName;
                user.PhoneNumber = model.PhoneNumber?.Trim();
                user.DateOfBirth = model.DateOfBirth;
                
                // Handle Gender - convert empty string to "Not Specified"
                if (string.IsNullOrWhiteSpace(model.Gender))
                {
                    user.Gender = "Not Specified";
                }
                else
                {
                    user.Gender = model.Gender.Trim();
                }

                if (profilePicture != null && profilePicture.Length > 0)
                {
                    try
                    {
                        var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                        Directory.CreateDirectory(uploads);

                        var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(profilePicture.FileName)}";
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(stream);
                        }

                        user.ProfilePicturePath = $"/uploads/profiles/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "Error uploading profile picture. Please try again.");
                        return View("Index", model);
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while updating your profile: {ex.Message}");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                try
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignore file deletion errors
                }

                user.ProfilePicturePath = null;
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] = "Profile picture removed successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeTheme(string theme)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new { error = "User not found" });

            if (theme != "dark" && theme != "light")
                return BadRequest(new { error = "Invalid theme" });

            user.ThemePreference = theme;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return Ok(new { success = true, theme = theme });
            }

            return BadRequest(new { error = "Failed to save theme" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest();

            var theme = user.ThemePreference ?? "dark";
            return Json(new { theme = theme });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Failed to delete account";
            return RedirectToAction("Index");
        }
    }
}

