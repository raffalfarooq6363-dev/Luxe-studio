using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;

namespace Luxe_glow_studio.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string recipient, string subject, string htmlBody);
        Task<bool> SendEmailToAdminsAsync(string subject, string htmlBody, IEnumerable<string>? additionalAdminEmails = null);
        Task<(bool isSuccess, string message)> SendTestEmailAsync(string targetEmail);
    }

    public sealed class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, AppDbContext context, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _env = env;
        }

        private SmtpSettings ReadSmtpSettingsFresh()
        {
            try
            {
                var emailSection = _configuration.GetSection("Email");
                return new SmtpSettings
                {
                    SmtpHost    = emailSection["SmtpHost"]?.Trim() ?? "",
                    SmtpPort    = emailSection.GetValue<int?>("SmtpPort") ?? 587,
                    EnableSsl   = emailSection.GetValue<bool?>("EnableSsl") ?? true,
                    Username    = emailSection["Username"]?.Trim() ?? "",
                    Password    = emailSection["Password"]?.Trim().Replace(" ", "") ?? "",
                    From        = emailSection["From"]?.Trim() ?? "",
                    FromName    = emailSection["FromName"]?.Trim() ?? "Luxe Glow Studio",
                    AdminEmail  = emailSection["AdminEmail"]?.Trim() ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read SMTP settings from appsettings.json");
                return new SmtpSettings();
            }
        }

        public async Task<bool> SendAsync(string recipient, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger.LogWarning("Email recipient is empty. Subject: {Subject}", subject);
                return false;
            }

            // Read FRESH from file so settings saved via admin UI work without restart
            var smtp = ReadSmtpSettingsFresh();

            if (string.IsNullOrWhiteSpace(smtp.SmtpHost) || string.IsNullOrWhiteSpace(smtp.From))
            {
                _logger.LogWarning(
                    "[EMAIL NOT CONFIGURED] SMTP host or From address is empty in appsettings.json. " +
                    "Go to Admin Dashboard -> Email Settings, fill Gmail credentials and click Save. " +
                    "Tried to send to: {Recipient} | Subject: {Subject}",
                    recipient, subject);
                return false;
            }

            try
            {
                using var message = new MailMessage(
                    new MailAddress(smtp.From, smtp.FromName),
                    new MailAddress(recipient))
                {
                    Subject    = subject,
                    Body       = htmlBody,
                    IsBodyHtml = true
                };

                using var client = new SmtpClient(smtp.SmtpHost, smtp.SmtpPort)
                {
                    EnableSsl            = smtp.EnableSsl,
                    DeliveryMethod       = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(smtp.Username))
                {
                    client.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation(
                    "Email successfully dispatched to {Recipient} with subject '{Subject}'.",
                    recipient, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email to {Recipient}. SMTP: {Host}:{Port} | From: {From} | Error: {Message}",
                    recipient, smtp.SmtpHost, smtp.SmtpPort, smtp.From, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendEmailToAdminsAsync(string subject, string htmlBody,
            IEnumerable<string>? additionalAdminEmails = null)
        {
            var smtp       = ReadSmtpSettingsFresh();
            var adminEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. AdminEmail from config (fresh file)
            if (!string.IsNullOrWhiteSpace(smtp.AdminEmail))
            {
                foreach (var email in smtp.AdminEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = email.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        adminEmails.Add(trimmed);
                }
            }

            // 2. Active Admin users from DB
            try
            {
                var dbAdmins = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == "Admin" && u.IsActive && !string.IsNullOrWhiteSpace(u.Email))
                    .Select(u => u.Email)
                    .ToListAsync();

                foreach (var email in dbAdmins)
                    if (!string.IsNullOrWhiteSpace(email))
                        adminEmails.Add(email.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch admin emails from database.");
            }

            // 3. Additional emails passed in
            if (additionalAdminEmails != null)
                foreach (var email in additionalAdminEmails)
                    if (!string.IsNullOrWhiteSpace(email))
                        adminEmails.Add(email.Trim());

            // Fallback: use From address
            if (adminEmails.Count == 0 && !string.IsNullOrWhiteSpace(smtp.From))
                adminEmails.Add(smtp.From.Trim());

            if (adminEmails.Count == 0)
            {
                _logger.LogWarning("No admin email found. Skipping admin notification.");
                return false;
            }

            bool atLeastOneSuccess = false;
            foreach (var email in adminEmails)
            {
                var success = await SendAsync(email, subject, htmlBody);
                if (success) atLeastOneSuccess = true;
            }

            return atLeastOneSuccess;
        }

        public async Task<(bool isSuccess, string message)> SendTestEmailAsync(string targetEmail)
        {
            var smtp = ReadSmtpSettingsFresh();
            if (string.IsNullOrWhiteSpace(smtp.SmtpHost) || string.IsNullOrWhiteSpace(smtp.From))
            {
                return (false,
                    "SMTP is not configured. Go to Admin Dashboard -> Email Settings and fill in: " +
                    "SMTP Host (smtp.gmail.com), Port (587), Username (your Gmail), " +
                    "App Password (16-digit from Google Account security), From Email (same Gmail), then click Save.");
            }

            var subject = "Luxe Glow Studio - Test Email Notification";
            var body    = @"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;
                            border:1px solid #eedad3;border-radius:12px;background:#fdfcfb;'>
                    <div style='text-align:center;padding-bottom:16px;border-bottom:1px solid #eedad3;'>
                        <h1 style='color:#742044;margin:0;font-size:24px;letter-spacing:1px;'>LUXE GLOW STUDIO</h1>
                        <p style='color:#a37a78;margin:4px 0 0 0;font-size:13px;'>Beauty &amp; Aesthetics</p>
                    </div>
                    <div style='padding:24px 0;'>
                        <h2 style='color:#2c1825;font-size:18px;'>SMTP Email System Working!</h2>
                        <p style='color:#55434d;font-size:14px;line-height:1.6;'>
                            Your SMTP settings are correctly configured.
                            Customers will now receive real booking confirmation emails in their Gmail inbox.
                        </p>
                    </div>
                    <div style='border-top:1px solid #eedad3;padding-top:16px;font-size:12px;
                                color:#8f7b86;text-align:center;'>
                        &copy; Luxe Glow Studio. All rights reserved.
                    </div>
                </div>";

            try
            {
                var sent = await SendAsync(targetEmail, subject, body);
                return sent
                    ? (true,  $"Test email sent successfully to {targetEmail}!")
                    : (false, $"Failed to send test email to {targetEmail}. " +
                              "Check your Gmail App Password and ensure 2-Step Verification is ON. See server logs.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }

    // Internal POCO to hold SMTP config read fresh from file
    internal sealed class SmtpSettings
    {
        public string SmtpHost   { get; set; } = "";
        public int    SmtpPort   { get; set; } = 587;
        public bool   EnableSsl  { get; set; } = true;
        public string Username   { get; set; } = "";
        public string Password   { get; set; } = "";
        public string From       { get; set; } = "";
        public string FromName   { get; set; } = "Luxe Glow Studio";
        public string AdminEmail { get; set; } = "";
    }
}