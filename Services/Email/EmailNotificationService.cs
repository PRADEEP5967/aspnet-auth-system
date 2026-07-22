using AspNetAuthSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace AspNetAuthSystem.Services.Email
{
    /// <summary>
    /// Email notification service for user account events
    /// </summary>
    public interface IEmailNotificationService
    {
        Task SendRegistrationWelcomeEmailAsync(User user, string role, IEmailSender emailSender);
        Task SendAccountConfirmationEmailAsync(User user, string confirmationLink, IEmailSender emailSender);
        Task SendCourseEnrollmentNotificationAsync(string studentEmail, string studentName, string courseName, string teacherName, IEmailSender emailSender);
        Task SendCourseCreatedNotificationAsync(User teacher, string courseName, IEmailSender emailSender);
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendRegistrationWelcomeEmailAsync(User user, string role, IEmailSender emailSender)
        {
            try
            {
                await emailSender.SendWelcomeEmailAsync(user.Email ?? "", user.UserName ?? "", role);
                _logger.LogInformation("Welcome email sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to: {Email}", user.Email);
            }
        }

        public async Task SendAccountConfirmationEmailAsync(User user, string confirmationLink, IEmailSender emailSender)
        {
            try
            {
                await emailSender.SendConfirmationEmailAsync(
                    user.Email ?? "",
                    confirmationLink,
                    user.UserName ?? ""
                );
                _logger.LogInformation("Confirmation email sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email to: {Email}", user.Email);
            }
        }

        public async Task SendCourseEnrollmentNotificationAsync(
            string studentEmail,
            string studentName,
            string courseName,
            string teacherName,
            IEmailSender emailSender)
        {
            try
            {
                var subject = $"Enrolled in {courseName}";
                var message = $@"
                    <html>
                    <body style="font-family: Arial, sans-serif;">
                        <h2>Course Enrollment Confirmation</h2>
                        <p>Hello {studentName},</p>
                        <p>You have successfully enrolled in <strong>{courseName}</strong></p>
                        <p>Instructor: <strong>{teacherName}</strong></p>
                        <p>You can now access the course content in your dashboard.</p>
                    </body>
                    </html>
                ";

                await emailSender.SendEmailAsync(studentEmail, subject, message);
                _logger.LogInformation("Course enrollment notification sent to: {Email}", studentEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending enrollment notification to: {Email}", studentEmail);
            }
        }

        public async Task SendCourseCreatedNotificationAsync(User teacher, string courseName, IEmailSender emailSender)
        {
            try
            {
                var subject = $"Course Created: {courseName}";
                var message = $@"
                    <html>
                    <body style="font-family: Arial, sans-serif;">
                        <h2>Course Creation Confirmation</h2>
                        <p>Hello {teacher.FirstName},</p>
                        <p>Your course <strong>{courseName}</strong> has been successfully created!</p>
                        <p>You can now manage the course and upload content.</p>
                        <p>Students will be able to find and enroll in your course.</p>
                    </body>
                    </html>
                ";

                await emailSender.SendEmailAsync(teacher.Email ?? "", subject, message);
                _logger.LogInformation("Course creation notification sent to: {Email}", teacher.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending course creation notification to: {Email}", teacher.Email);
            }
        }
    }
}