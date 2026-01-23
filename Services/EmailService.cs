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

                _logger.LogInformation("SMTP Config Check - Server: {Server}, Email: {Email}, Password: {HasPassword}, Port: {Port}",
                    string.IsNullOrEmpty(smtpServer) ? "NOT SET" : smtpServer,
                    string.IsNullOrEmpty(senderEmail) ? "NOT SET" : senderEmail,
                    string.IsNullOrEmpty(senderPassword) ? "NOT SET" : "***SET***",
                    smtpPortStr);

                // Skip email sending if SMTP is not configured
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogWarning("SMTP configuration is incomplete. Email will not be sent. Please set environment variables: SmtpSettings__Server, SmtpSettings__SenderEmail, SmtpSettings__SenderPassword");
                    return;
                }

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
