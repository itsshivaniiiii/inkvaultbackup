using InkVault.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InkVault.Services
{
    public interface IBirthdayService
    {
        Task SendBirthdayEmailsAsync();
    }

    public class BirthdayService : IBirthdayService
    {
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BirthdayService> _logger;

        public BirthdayService(
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<BirthdayService> logger)
        {
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SendBirthdayEmailsAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var today = DateTime.Today;

                _logger.LogInformation($"Birthday service running at {DateTime.UtcNow}. Checking for birthdays on {today:yyyy-MM-dd}");

                foreach (var user in users)
                {
                    // Skip if user has no DateOfBirth
                    if (user.DateOfBirth == null)
                        continue;

                    // Get user's birth month and day
                    int birthMonth = user.DateOfBirth.Value.Month;
                    int birthDay = user.DateOfBirth.Value.Day;

                    // Check if TODAY is the user's birthday (same month and day, ignoring year)
                    bool isBirthdayToday = (today.Month == birthMonth && today.Day == birthDay);

                    if (!isBirthdayToday)
                        continue; // Not this user's birthday, skip

                    // Email already sent today? Skip to prevent duplicate emails
                    bool alreadySentToday = user.LastBirthdayEmailSent.HasValue && 
                                          user.LastBirthdayEmailSent.Value.Date == today;

                    if (alreadySentToday)
                    {
                        _logger.LogInformation($"Birthday email already sent to {user.Email} ({user.UserName}) on {today:yyyy-MM-dd}");
                        continue;
                    }

                    // This is their birthday AND we haven't sent email today - Send it!
                    _logger.LogInformation($"?? Birthday detected for {user.Email} ({user.UserName}) on {today:yyyy-MM-dd}");

                    // Calculate age
                    int age = today.Year - user.DateOfBirth.Value.Year;
                    if (user.DateOfBirth.Value.Date > today.AddYears(-age))
                        age--;

                    // Send birthday email
                    await SendBirthdayEmailAsync(user, age);

                    // Mark email as sent TODAY (prevent duplicate sends)
                    user.LastBirthdayEmailSent = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation($"? Birthday email sent successfully to {user.Email} ({user.UserName}) - Turning {age}");
                }

                _logger.LogInformation("Birthday service check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error sending birthday emails: {ex.Message}");
            }
        }

        private async Task SendBirthdayEmailAsync(ApplicationUser user, int age)
        {
            string subject = $"?? Happy Birthday, {user.FirstName}! ??";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 2.5em;
            margin-bottom: 10px;
        }}
        .header p {{
            margin: 0;
            font-size: 1.1em;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 30px;
            text-align: center;
            color: #333;
        }}
        .age-badge {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 30px;
            border-radius: 50px;
            font-size: 1.3em;
            font-weight: bold;
            margin: 20px 0;
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.3);
        }}
        .message {{
            font-size: 1.1em;
            line-height: 1.8;
            color: #555;
            margin: 20px 0;
        }}
        .message strong {{
            color: #667eea;
        }}
        .emoji {{
            font-size: 2em;
            margin: 0 5px;
        }}
        .footer {{
            background: #f5f5f5;
            padding: 20px;
            text-align: center;
            color: #888;
            font-size: 0.9em;
            border-top: 1px solid #eee;
        }}
        .footer a {{
            color: #667eea;
            text-decoration: none;
        }}
        .footer a:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Happy Birthday! ??</h1>
            <p>Wishing you an absolutely amazing day!</p>
        </div>
        <div class='content'>
            <p class='message'>
                <span class='emoji'>??</span>
                <strong>Dear {user.FirstName},</strong>
                <span class='emoji'>??</span>
            </p>
            <p class='message'>
                Today is <strong>YOUR</strong> special day, and we want you to know how much you mean to us!
            </p>
            <div class='age-badge'>
                You're turning <strong>{age}</strong>! ??
            </div>
            <p class='message'>
                <span class='emoji'>?</span>
                Another year around the sun! Thank you for sharing your creativity, thoughts, and wonderful stories with us on InkVault. 
                <span class='emoji'>?</span>
            </p>
            <p class='message'>
                We hope your birthday is filled with:
                <br><span class='emoji'>??</span> Love and laughter
                <br><span class='emoji'>??</span> Unforgettable moments
                <br><span class='emoji'>??</span> Dreams coming true
                <br><span class='emoji'>??</span> Endless happiness
            </p>
            <p class='message'>
                <strong>May this year bring you new adventures, wonderful memories, and endless possibilities!</strong>
            </p>
            <p class='message'>
                With all our love and best wishes,<br>
                <strong>The InkVault Family</strong> ??
            </p>
        </div>
        <div class='footer'>
            <p>
                © 2026 InkVault - Secure Your Thoughts<br>
                This is an automated birthday message sent with lots of affection and love! ??
            </p>
        </div>
    </div>
</body>
</html>
";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send birthday email to {user.Email}: {ex.Message}");
            }
        }
    }
}
