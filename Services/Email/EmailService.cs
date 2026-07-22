namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// Email service for sending various types of emails
    /// This service orchestrates all email sending operations
    /// </summary>
    public interface IEmailService
    {
        Task SendSimpleEmailAsync(string to, string subject, string body);
        Task SendPasswordResetAsync(string to, string userName, string resetLink);
        Task SendWelcomeAsync(string to, string userName, string role);
        Task SendNotificationAsync(string to, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEmailSender emailSender, ILogger<EmailService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendSimpleEmailAsync(string to, string subject, string body)
        {
            try
            {
                await _emailSender.SendEmailAsync(to, subject, body);
                _logger.LogInformation("Simple email sent to: {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending simple email to: {Email}", to);
                throw;
            }
        }

        public async Task SendPasswordResetAsync(string to, string userName, string resetLink)
        {
            try
            {
                await _emailSender.SendPasswordResetEmailAsync(to, resetLink, userName);
                _logger.LogInformation("Password reset email sent to: {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to: {Email}", to);
                throw;
            }
        }

        public async Task SendWelcomeAsync(string to, string userName, string role)
        {
            try
            {
                await _emailSender.SendWelcomeEmailAsync(to, userName, role);
                _logger.LogInformation("Welcome email sent to: {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to: {Email}", to);
                throw;
            }
        }

        public async Task SendNotificationAsync(string to, string subject, string message)
        {
            try
            {
                await _emailSender.SendEmailAsync(to, subject, message);
                _logger.LogInformation("Notification email sent to: {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification email to: {Email}", to);
                throw;
            }
        }
    }
}