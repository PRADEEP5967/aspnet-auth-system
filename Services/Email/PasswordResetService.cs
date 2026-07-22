using AspNetAuthSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// Password reset service to handle password reset logic
    /// </summary>
    public interface IPasswordResetService
    {
        Task<(bool Success, string Message)> RequestPasswordResetAsync(string email, IEmailSender emailSender, IUrlHelper urlHelper, HttpRequest request);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword, UserManager<User> userManager);
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(UserManager<User> userManager, ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> RequestPasswordResetAsync(
            string email, 
            IEmailSender emailSender, 
            IUrlHelper urlHelper,
            HttpRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    // Don't reveal if email exists for security reasons
                    return (true, "If an account with this email exists, a password reset link has been sent.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var scheme = request.Scheme;
                var host = request.Host.Value;
                var resetLink = $"{scheme}://{host}/Account/ResetPassword?userId={user.Id}&code={Uri.EscapeDataString(token)}";

                await emailSender.SendPasswordResetEmailAsync(email, resetLink, user.UserName ?? email);

                _logger.LogInformation("Password reset email sent to: {Email}", email);
                return (true, "Password reset email sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
                return (false, "An error occurred while sending the password reset email. Please try again later.");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(
            string email, 
            string token, 
            string newPassword,
            UserManager<User> userManager)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", email);
                    return (false, "User not found.");
                }

                var result = await userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
                    return (true, "Your password has been reset successfully!");
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                return (false, $"Password reset failed: {errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", email);
                return (false, "An error occurred while resetting your password. Please try again later.");
            }
        }
    }
}