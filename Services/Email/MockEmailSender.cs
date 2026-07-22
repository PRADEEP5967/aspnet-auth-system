namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// Mock email sender for development/testing purposes
    /// Logs emails instead of sending them
    /// </summary>
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("========== MOCK EMAIL SENT ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Message: {Message}", message);
            _logger.LogInformation("=====================================");
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            _logger.LogInformation("========== PASSWORD RESET EMAIL ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("User: {UserName}", userName);
            _logger.LogInformation("Reset Link: {ResetLink}", resetLink);
            _logger.LogInformation("=========================================");
            return Task.CompletedTask;
        }

        public Task SendConfirmationEmailAsync(string email, string confirmationLink, string userName)
        {
            _logger.LogInformation("========== EMAIL CONFIRMATION ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("User: {UserName}", userName);
            _logger.LogInformation("Confirmation Link: {ConfirmationLink}", confirmationLink);
            _logger.LogInformation("========================================");
            return Task.CompletedTask;
        }

        public Task SendWelcomeEmailAsync(string email, string userName, string role)
        {
            _logger.LogInformation("========== WELCOME EMAIL ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("User: {UserName}", userName);
            _logger.LogInformation("Role: {Role}", role);
            _logger.LogInformation("====================================");
            return Task.CompletedTask;
        }
    }
}