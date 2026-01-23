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
            
            var subject = "Your OTP for InkVault";
            var body = $@"
                <html>
                <body>
                    <h2>InkVault - One-Time Password</h2>
                    <p>Your OTP is: <strong>{otp}</strong></p>
                    <p>This OTP will expire in 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
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
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpServer = smtpSettings["Server"];
                var senderEmail = smtpSettings["SenderEmail"];
                var senderPassword = smtpSettings["SenderPassword"];

                // Skip email sending if SMTP is not configured (graceful degradation)
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogWarning("SMTP configuration is incomplete. Skipping email send. Server={Server}, Email={Email}", 
                        smtpServer ?? "null", senderEmail ?? "null");
                    return;
                }

                var smtpPortStr = smtpSettings["Port"] ?? "587";
                if (!int.TryParse(smtpPortStr, out var smtpPort))
                {
                    smtpPort = 587;
                }

                _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}", smtpServer, smtpPort);

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
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    
                    _logger.LogInformation("Sending email to {Email}", email);
                    await client.SendAsync(message);
                    
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email to {Email}. This is non-critical and won't affect user experience.", email);
                // Email is non-blocking - do not throw
            }
        }
    }
}
