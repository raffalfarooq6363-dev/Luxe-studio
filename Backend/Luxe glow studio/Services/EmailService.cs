using System.Net;
using System.Net.Mail;

namespace Luxe_glow_studio.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string recipient, string subject, string htmlBody);
    }

    public sealed class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string recipient, string subject, string htmlBody)
        {
            var host = _configuration["Email:SmtpHost"];
            var from = _configuration["Email:From"];
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("Email is not configured. Skipping email to {Recipient} with subject {Subject}.", recipient, subject);
                return false;
            }

            try
            {
                using var message = new MailMessage(from, recipient, subject, htmlBody) { IsBodyHtml = true };
                using var client = new SmtpClient(host, _configuration.GetValue<int>("Email:SmtpPort", 587))
                {
                    EnableSsl = _configuration.GetValue("Email:EnableSsl", true),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];
                if (!string.IsNullOrWhiteSpace(username))
                {
                    client.Credentials = new NetworkCredential(username, password);
                }

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}.", recipient);
                return false;
            }
        }
    }
}