using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TripGenius.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                // 🔍 Read SMTP settings
                var host = _config["SmtpSettings:Host"];
                var portStr = _config["SmtpSettings:Port"];
                var emailFrom = _config["SmtpSettings:Email"];
                var password = _config["SmtpSettings:Password"];

                // ❗ Validate config
                if (string.IsNullOrEmpty(host) ||
                    string.IsNullOrEmpty(portStr) ||
                    string.IsNullOrEmpty(emailFrom) ||
                    string.IsNullOrEmpty(password))
                {
                    throw new Exception("SMTP configuration is missing in appsettings.json");
                }

                // 🔢 Convert port safely
                if (!int.TryParse(portStr, out int port))
                {
                    throw new Exception("SMTP Port is invalid");
                }

                // 📧 Create email
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("TripGenius", emailFrom));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                email.Body = new TextPart("html")
                {
                    Text = htmlMessage
                };

                // 📡 Send email
                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(emailFrom, password);

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // ❌ Show detailed error in debug
                Console.WriteLine("EMAIL ERROR: " + ex.Message);
                throw; // rethrow so controller can log it
            }
        }
    }
}