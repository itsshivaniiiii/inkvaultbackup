using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace InkVault.Services
{
    public interface IEmailService
    {
        Task SendOTPAsync(string email, string otp);
        Task SendEmailAsync(string email, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOTPAsync(string email, string otp)
        {
            _logger.LogInformation("Preparing to send OTP email to {Email}", email);
            
            var subject = "Email Verification - InkVault";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }}
        .wrapper {{ background: #f5f5f5; padding: 40px 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 10px 40px rgba(102, 126, 234, 0.15); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 60px 30px; text-align: center; color: white; }}
        .header h1 {{ font-size: 36px; font-weight: 700; margin: 0; margin-bottom: 10px; letter-spacing: -0.5px; }}
        .header p {{ font-size: 16px; opacity: 0.95; margin: 0; }}
        .content {{ padding: 40px 30px; }}
        .greeting {{ font-size: 16px; color: #333; margin-bottom: 20px; line-height: 1.8; }}
        .otp-section {{ background: #f0f0f9; border: 2px solid #e0e0f0; border-radius: 12px; padding: 30px; margin: 30px 0; text-align: center; }}
        .otp-label {{ font-size: 12px; font-weight: 700; color: #667eea; letter-spacing: 1px; margin-bottom: 20px; text-transform: uppercase; }}
        .otp-code {{ font-size: 48px; font-weight: 700; color: #667eea; letter-spacing: 8px; margin: 20px 0; font-family: 'Courier New', monospace; }}
        .otp-expiry {{ font-size: 14px; color: #666; margin-top: 15px; }}
        .otp-expiry-time {{ color: #d32f2f; font-weight: 700; }}
        .warning {{ font-size: 13px; color: #999; margin-top: 10px; }}
        .how-to-use {{ background: #f9f9f9; border-left: 4px solid #667eea; padding: 20px; margin: 30px 0; border-radius: 4px; }}
        .how-to-use h3 {{ font-size: 14px; font-weight: 700; color: #333; margin-bottom: 10px; }}
        .how-to-use p {{ font-size: 14px; color: #666; margin: 0; line-height: 1.8; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #999; border-top: 1px solid #eee; }}
        .divider {{ height: 1px; background: #eee; margin: 30px 0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <!-- Header -->
            <div class='header'>
                <h1>Email Verification</h1>
                <p>Confirm your InkVault account</p>
            </div>

            <!-- Content -->
            <div class='content'>
                <p class='greeting'>Hi there,</p>
                <p class='greeting'>Thank you for signing up with InkVault! To complete your account verification, please use the one-time password below:</p>

                <!-- OTP Section -->
                <div class='otp-section'>
                    <div class='otp-label'>Your One-Time Password (OTP)</div>
                    <div class='otp-code'>{otp}</div>
                    <div class='otp-expiry'>
                        This code will expire in <span class='otp-expiry-time'>10 minutes</span>
                    </div>
                    <div class='warning'>Do not share this code with anyone</div>
                </div>

                <!-- How to Use -->
                <div class='how-to-use'>
                    <h3>How to use:</h3>
                    <p>Enter this one-time password (OTP) on the verification page to complete your email verification and gain full access to InkVault.</p>
                </div>

                <p style='color: #999; font-size: 13px; margin-top: 30px;'>If you didn't create this account, you can safely ignore this email.</p>

                <div class='divider'></div>

                <p style='color: #999; font-size: 12px; margin-top: 20px;'>Need help? Contact our support team at support@inkvault.com</p>
            </div>

            <!-- Footer -->
            <div class='footer'>
                <p>© 2025 InkVault. All rights reserved.</p>
                <p style='margin-top: 10px;'>This is an automated message. Please do not reply to this email address.</p>
            </div>
        </div>
    </div>
</body>
</html>";

            try
            {
                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send OTP email to {Email}. User can still verify via manual entry.", email);
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlBody)
        {
            try
            {
                // Try multiple configuration sources
                var smtpServer = _configuration["SmtpSettings:Server"] 
                    ?? Environment.GetEnvironmentVariable("SmtpSettings__Server")
                    ?? Environment.GetEnvironmentVariable("SMTP_SERVER");
                    
                var senderEmail = _configuration["SmtpSettings:SenderEmail"] 
                    ?? Environment.GetEnvironmentVariable("SmtpSettings__SenderEmail")
                    ?? Environment.GetEnvironmentVariable("SMTP_EMAIL");
                    
                var senderPassword = _configuration["SmtpSettings:SenderPassword"] 
                    ?? Environment.GetEnvironmentVariable("SmtpSettings__SenderPassword")
                    ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD");
                    
                var smtpPortStr = _configuration["SmtpSettings:Port"] 
                    ?? Environment.GetEnvironmentVariable("SmtpSettings__Port")
                    ?? Environment.GetEnvironmentVariable("SMTP_PORT")
                    ?? "587";

                Console.WriteLine($"[EMAIL] ========== EMAIL SERVICE DEBUG ==========");
                Console.WriteLine($"[EMAIL] Recipient: {email}");
                Console.WriteLine($"[EMAIL] Subject: {subject}");
                Console.WriteLine($"[EMAIL] SMTP Server: {(string.IsNullOrEmpty(smtpServer) ? "[NOT SET]" : $"[OK] {smtpServer}")}");
                Console.WriteLine($"[EMAIL] Sender Email: {(string.IsNullOrEmpty(senderEmail) ? "[NOT SET]" : $"[OK] {senderEmail}")}");
                Console.WriteLine($"[EMAIL] Sender Password: {(string.IsNullOrEmpty(senderPassword) ? "[NOT SET]" : "[OK] SET")}");
                Console.WriteLine($"[EMAIL] SMTP Port: {smtpPortStr}");
                Console.WriteLine($"[EMAIL] ==========================================");

                _logger.LogInformation("SMTP Config Check - Server: {Server}, Email: {Email}, Password: {HasPassword}, Port: {Port}",
                    string.IsNullOrEmpty(smtpServer) ? "NOT SET" : smtpServer,
                    string.IsNullOrEmpty(senderEmail) ? "NOT SET" : senderEmail,
                    string.IsNullOrEmpty(senderPassword) ? "NOT SET" : "***SET***",
                    smtpPortStr);

                // Skip email sending if SMTP is not configured
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    Console.WriteLine("[EMAIL] SMTP CONFIGURATION INCOMPLETE!");
                    Console.WriteLine("[EMAIL] Please set these environment variables:");
                    Console.WriteLine("[EMAIL]   - SMTP_SERVER or SmtpSettings__Server");
                    Console.WriteLine("[EMAIL]   - SMTP_EMAIL or SmtpSettings__SenderEmail");
                    Console.WriteLine("[EMAIL]   - SMTP_PASSWORD or SmtpSettings__SenderPassword");
                    Console.WriteLine("[EMAIL]   - SMTP_PORT or SmtpSettings__Port (optional, default: 587)");
                    _logger.LogWarning("SMTP configuration is incomplete. Email will not be sent. Please set environment variables: SmtpSettings__Server, SmtpSettings__SenderEmail, SmtpSettings__SenderPassword");
                    return;
                }

                if (!int.TryParse(smtpPortStr, out var smtpPort))
                {
                    smtpPort = 587;
                }

                _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}", smtpServer, smtpPort);
                Console.WriteLine($"[EMAIL] Connecting to {smtpServer}:{smtpPort}...");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("InkVault", senderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Set a timeout of 30 seconds for SMTP operations
                    client.Timeout = 30000;

                    // Connect with timeout handling
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    
                    _logger.LogInformation("Connected to SMTP server, authenticating...");
                    Console.WriteLine($"[EMAIL] [OK] Connected! Authenticating...");
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    
                    _logger.LogInformation("Sending email to {Email}", email);
                    Console.WriteLine($"[EMAIL] [OK] Authenticated! Sending email to {email}...");
                    await client.SendAsync(message);
                    
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                    Console.WriteLine($"[EMAIL] [OK] EMAIL SENT SUCCESSFULLY to {email}!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL] ERROR: {ex.Message}");
                Console.WriteLine($"[EMAIL] Stack: {ex.StackTrace}");
                _logger.LogWarning(ex, "Failed to send email to {Email}. This is non-critical and won't affect user experience.", email);
                // Email is non-blocking - do not throw
            }
        }
    }
}
