using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using InkVault.Models;

namespace InkVault.Filters
{
    public class FirstLoginFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public FirstLoginFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if user is authenticated
            if (context.HttpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(context.HttpContext.User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        // Pass first login status to all views
                        // This will show Getting Started section throughout the entire first login session
                        bool isFirstLoginSession = !user.HasCompletedFirstLogin;
                        
                        if (context.Controller is Controller controller)
                        {
                            controller.ViewData["IsFirstLogin"] = isFirstLoginSession;
                        }
                    }
                }
            }

            await next();
        }
    }
}
