using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace InkVault.Utilities
{
    /// <summary>
    /// Email Configuration Diagnostic Tool
    /// Run this to verify email configuration is working
    /// </summary>
    public class EmailDiagnostic
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly string _recipientEmail;

        public EmailDiagnostic(
            string smtpServer,
            int smtpPort,
            string senderEmail,
            string senderPassword,
            string recipientEmail)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _senderEmail = senderEmail;
            _senderPassword = senderPassword;
            _recipientEmail = recipientEmail;
        }

        public async Task<bool> TestConnectionAsync()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("EMAIL DIAGNOSTIC - SMTP CONNECTION TEST");
            Console.WriteLine(new string('=', 60));

            // Step 1: Validate configuration
            Console.WriteLine("\n[STEP 1] Validating Configuration...");
            if (string.IsNullOrEmpty(_smtpServer))
            {
                Console.WriteLine("? SMTP Server is EMPTY");
                return false;
            }
            if (string.IsNullOrEmpty(_senderEmail))
            {
                Console.WriteLine("? Sender Email is EMPTY");
                return false;
            }
            if (string.IsNullOrEmpty(_senderPassword))
            {
                Console.WriteLine("? Sender Password is EMPTY");
                return false;
            }
            if (_senderPassword.Contains(" "))
            {
                Console.WriteLine("? Password has SPACES (INVALID!)");
                Console.WriteLine($"   Password: {_senderPassword}");
                return false;
            }

            Console.WriteLine("? Configuration validation passed");
            Console.WriteLine($"   Server: {_smtpServer}");
            Console.WriteLine($"   Port: {_smtpPort}");
            Console.WriteLine($"   Email: {_senderEmail}");
            Console.WriteLine($"   Password: {_senderPassword.Length} chars (no spaces)");

            // Step 2: Test SMTP Connection
            Console.WriteLine("\n[STEP 2] Testing SMTP Connection...");
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Timeout = 30000; // 30 seconds

                    Console.WriteLine($"   Connecting to {_smtpServer}:{_smtpPort}...");
                    await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    Console.WriteLine("   ? Connected successfully");

                    Console.WriteLine($"   Authenticating as {_senderEmail}...");
                    await client.AuthenticateAsync(_senderEmail, _senderPassword);
                    Console.WriteLine("   ? Authentication successful");

                    await client.DisconnectAsync(true);
                    Console.WriteLine("   ? Disconnected gracefully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? SMTP Connection Failed!");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                return false;
            }

            // Step 3: Test Email Sending
            Console.WriteLine("\n[STEP 3] Testing Email Send...");
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("InkVault Test", _senderEmail));
                message.To.Add(new MailboxAddress("", _recipientEmail));
                message.Subject = "InkVault Email Test";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial; color: #333;'>
                        <h2>? Email Configuration Working!</h2>
                        <p>If you received this email, your email configuration is correct.</p>
                        <hr />
                        <p><strong>Test Details:</strong></p>
                        <ul>
                            <li>From: {_senderEmail}</li>
                            <li>To: {_recipientEmail}</li>
                            <li>Time: {DateTime.UtcNow:F}</li>
                            <li>SMTP Server: {_smtpServer}</li>
                            <li>Port: {_smtpPort}</li>
                        </ul>
                        <p>You can now use OTP and email notifications in InkVault.</p>
                        <p>Best regards,<br/>InkVault Email System</p>
                    </body>
                    </html>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Timeout = 30000;
                    await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_senderEmail, _senderPassword);

                    Console.WriteLine($"   Sending test email to {_recipientEmail}...");
                    await client.SendAsync(message);
                    Console.WriteLine($"   ? Email sent successfully!");

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Email Send Failed!");
                Console.WriteLine($"   Error: {ex.Message}");
                return false;
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("? ALL TESTS PASSED - Email is working correctly!");
            Console.WriteLine(new string('=', 60) + "\n");
            return true;
        }
    }
}
