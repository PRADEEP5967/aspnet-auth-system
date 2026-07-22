using AspNetAuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetAuthSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<User> _userManager;

        public DashboardController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = $"{user?.FirstName} {user?.LastName}";
            return View();
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Teacher()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = $"{user?.FirstName} {user?.LastName}";
            return View();
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Student()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["UserName"] = $"{user?.FirstName} {user?.LastName}";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
    }
}