using AspNetAuthSystem.DTOs;
using AspNetAuthSystem.Models;
using AspNetAuthSystem.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetAuthSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            IEmailSender emailSender,
            IEmailNotificationService emailNotificationService,
            IPasswordResetService passwordResetService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _emailNotificationService = emailNotificationService;
            _passwordResetService = passwordResetService;
            _logger = logger;
        }

        // Student Registration - Public access
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    try
                    {
                        // Assign Student role only
                        if (!await _roleManager.RoleExistsAsync("Student"))
                        {
                            await _roleManager.CreateAsync(new Role { Name = "Student", NormalizedName = "STUDENT" });
                        }
                        await _userManager.AddToRoleAsync(user, "Student");

                        // Send welcome email
                        await _emailNotificationService.SendRegistrationWelcomeEmailAsync(user, "Student", _emailSender);

                        _logger.LogInformation("Student user {UserId} registered successfully", user.Id);

                        // Sign in the user
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        
                        TempData["Success"] = "Registration successful! Welcome to ASP.NET Auth System.";
                        return RedirectToAction("Index", "Home");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during post-registration process for user {UserId}", user.Id);
                        ModelState.AddModelError(string.Empty, "Registration completed but an error occurred. Please try logging in.");
                    }
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Registration error for user {Username}: {Error}", model.UserName, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find user by email or username
                var user = await _userManager.FindByEmailAsync(model.EmailOrUsername) ??
                          await _userManager.FindByNameAsync(model.EmailOrUsername);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName!,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                        TempData["Success"] = $"Welcome back, {user.FirstName}!";
                        return RedirectToLocal(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User {UserId} account locked due to failed attempts", user.Id);
                        TempData["Error"] = "Account locked due to multiple failed attempts. Please try again later.";
                        return RedirectToAction(nameof(Login));
                    }

                    if (result.RequiresTwoFactor)
                    {
                        _logger.LogInformation("User {UserId} requires two-factor authentication", user.Id);
                        return RedirectToAction(nameof(LoginWith2fa));
                    }

                    _logger.LogWarning("Invalid login attempt for user {UserId}", user.Id);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                else
                {
                    _logger.LogWarning("Login attempt with non-existent email/username: {EmailOrUsername}", model.EmailOrUsername);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // Admin Teacher Management
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateTeacher()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(RegisterDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    try
                    {
                        // Assign Teacher role
                        if (!await _roleManager.RoleExistsAsync("Teacher"))
                        {
                            await _roleManager.CreateAsync(new Role { Name = "Teacher", NormalizedName = "TEACHER" });
                        }
                        await _userManager.AddToRoleAsync(user, "Teacher");

                        _logger.LogInformation("Teacher user {UserId} created successfully by Admin", user.Id);
                        TempData["Success"] = $"Teacher '{model.UserName}' created successfully.";
                        return RedirectToAction(nameof(ManageTeachers));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating teacher user {UserId}", user.Id);
                        ModelState.AddModelError(string.Empty, "Error occurred while creating teacher. Please try again.");
                    }
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Teacher creation error: {Error}", error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Admin Teacher Management List
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ManageTeachers()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return View(teachers.Cast<User>().ToList());
        }

        // Admin Delete Teacher
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _userManager.FindByIdAsync(id.ToString());
            if (teacher != null)
            {
                var result = await _userManager.DeleteAsync(teacher);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Teacher {UserId} deleted successfully by Admin", teacher.Id);
                    TempData["Success"] = "Teacher deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Error deleting teacher.";
                }
            }
            return RedirectToAction(nameof(ManageTeachers));
        }

        // Teacher Role Management - Teachers manage their assigned courses and students
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public IActionResult TeacherDashboard()
        {
            return View();
        }

        // Password Reset
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required");
                return View();
            }

            try
            {
                var (success, message) = await _passwordResetService.RequestPasswordResetAsync(
                    email,
                    _emailSender,
                    Url,
                    Request
                );

                if (success)
                {
                    _logger.LogInformation("Password reset requested for email: {Email}", email);
                    TempData["Success"] = message;
                }
                else
                {
                    _logger.LogWarning("Password reset failed for email: {Email}", email);
                    TempData["Error"] = message;
                }

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
                TempData["Error"] = "An error occurred. Please try again later.";
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(int? userId, string? code = null)
        {
            if (code == null || userId == null)
            {
                return BadRequest("A code and user ID must be supplied for password reset.");
            }

            var model = new ResetPasswordDTO { Token = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, message) = await _passwordResetService.ResetPasswordAsync(
                    model.Email,
                    model.Token,
                    model.NewPassword,
                    _userManager
                );

                if (success)
                {
                    _logger.LogInformation("Password reset successful for email: {Email}", model.Email);
                    TempData["Success"] = message;
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                else
                {
                    _logger.LogWarning("Password reset failed for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred while resetting your password.");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LoginWith2fa(bool rememberMe)
        {
            ViewData["RememberMe"] = rememberMe;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(string code, bool rememberMe)
        {
            if (string.IsNullOrEmpty(code))
            {
                ModelState.AddModelError(string.Empty, "Verification code is required.");
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load two-factor authentication user.");
                return View();
            }

            var authenticatorCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked.", user.Id);
                ModelState.AddModelError(string.Empty, "Account locked due to multiple failed attempts.");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}