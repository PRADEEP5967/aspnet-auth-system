using System.Net;
using System.Net.Mail;

namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// SMTP Email sender service implementation
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = smtpSettings["SmtpHost"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUsername = smtpSettings["SmtpUsername"];
                var smtpPassword = smtpSettings["SmtpPassword"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                if (string.IsNullOrEmpty(smtpHost))
                {
                    _logger.LogWarning("SMTP settings not configured. Email not sent to: {Email}", email);
                    return;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail ?? smtpUsername, fromName ?? "ASP.NET Auth System"),
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to: {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            var subject = "Reset Your Password - ASP.NET Auth System";
            var message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border-radius: 5px; margin-top: 10px; }}
                        .button {{ background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px; }}
                        .footer {{ color: #666; font-size: 12px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class="content">
                            <p>Hello {userName},</p>
                            <p>We received a request to reset your password. Click the button below to reset it:</p>
                            <a href="{resetLink}" class="button">Reset Password</a>
                            <p>Or copy this link: <a href="{resetLink}">{resetLink}</a></p>
                            <p>If you didn't request this, you can ignore this email.</p>
                            <p>This link will expire in 24 hours.</p>
                        </div>
                        <div class="footer">
                            <p>&copy; 2024 ASP.NET Auth System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink, string userName)
        {
            var subject = "Confirm Your Email - ASP.NET Auth System";
            var message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border-radius: 5px; margin-top: 10px; }}
                        .button {{ background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px; }}
                        .footer {{ color: #666; font-size: 12px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <h2>Email Confirmation</h2>
                        </div>
                        <div class="content">
                            <p>Hello {userName},</p>
                            <p>Welcome to ASP.NET Auth System! Please confirm your email address by clicking the button below:</p>
                            <a href="{confirmationLink}" class="button">Confirm Email</a>
                            <p>Or copy this link: <a href="{confirmationLink}">{confirmationLink}</a></p>
                        </div>
                        <div class="footer">
                            <p>&copy; 2024 ASP.NET Auth System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName, string role)
        {
            var subject = "Welcome to ASP.NET Auth System";
            var dashboardUrl = role switch
            {
                "Admin" => "/Dashboard/Admin",
                "Teacher" => "/Dashboard/Teacher",
                "Student" => "/Dashboard/Student",
                _ => "/Dashboard/Student"
            };

            var message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; border-radius: 5px; margin-top: 10px; }}
                        .button {{ background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px; }}
                        .role-badge {{ background-color: #007bff; color: white; padding: 5px 10px; border-radius: 3px; display: inline-block; margin-top: 10px; }}
                        .footer {{ color: #666; font-size: 12px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <h2>Welcome to ASP.NET Auth System!</h2>
                        </div>
                        <div class="content">
                            <p>Hello {userName},</p>
                            <p>Thank you for registering! Your account has been successfully created.</p>
                            <p>Your Role: <span class="role-badge">{role}</span></p>
                            <p>You can now access your dashboard and start using the platform:</p>
                            <a href="https://localhost:7000{dashboardUrl}" class="button">Go to Dashboard</a>
                            <p>If you have any questions, please don't hesitate to contact us.</p>
                        </div>
                        <div class="footer">
                            <p>&copy; 2024 ASP.NET Auth System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, message);
        }
    }
}