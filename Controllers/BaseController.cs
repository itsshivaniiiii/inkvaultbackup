using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using InkVault.Models;

namespace InkVault.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected async Task SetFirstLoginViewData()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                
                // Show Getting Started if user has NOT completed first login
                // This will persist throughout the entire first login session
                bool isFirstLoginSession = user != null && !user.HasCompletedFirstLogin;
                ViewData["IsFirstLogin"] = isFirstLoginSession;
            }
        }
    }
}
