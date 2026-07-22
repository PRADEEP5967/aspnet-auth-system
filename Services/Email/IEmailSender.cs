namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// Interface for email sending service
    /// </summary>
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendPasswordResetEmailAsync(string email, string resetLink, string userName);
        Task SendConfirmationEmailAsync(string email, string confirmationLink, string userName);
        Task SendWelcomeEmailAsync(string email, string userName, string role);
    }
}