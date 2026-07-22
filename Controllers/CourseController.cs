using AspNetAuthSystem.DTOs;
using AspNetAuthSystem.Models;
using AspNetAuthSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetAuthSystem.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<User> _userManager;

        public CourseController(
            ICourseRepository courseRepository,
            UserManager<User> userManager)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllAsync();
            var courseDTOs = courses.Select(c => new CourseDTO
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Content = c.Content,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "Unknown",
                CreatedAt = c.CreatedAt
            }).ToList();

            return View(courseDTOs);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var courses = await _courseRepository.GetStudentCoursesAsync(user.Id);
            var courseDTOs = courses.Select(c => new CourseDTO
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Content = c.Content,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "Unknown",
                CreatedAt = c.CreatedAt
            }).ToList();

            return View(courseDTOs);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var course = new Course
                {
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content,
                    TeacherId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _courseRepository.AddAsync(course);
                await _courseRepository.SaveChangesAsync();

                TempData["Success"] = "Course created successfully!";
                return RedirectToAction(nameof(TeacherCourses));
            }

            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> TeacherCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var courses = await _courseRepository.GetCoursesByTeacherAsync(user.Id);
            var courseDTOs = courses.Select(c => new CourseDTO
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Content = c.Content,
                TeacherId = c.TeacherId,
                TeacherName = $"{user.FirstName} {user.LastName}",
                CreatedAt = c.CreatedAt
            }).ToList();

            return View(courseDTOs);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _courseRepository.GetCourseWithStudentsAsync(id.Value);
            if (course == null)
                return NotFound();

            var courseDTO = new CourseDTO
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Content = course.Content,
                TeacherId = course.TeacherId,
                TeacherName = course.Teacher != null ? $"{course.Teacher.FirstName} {course.Teacher.LastName}" : "Unknown",
                CreatedAt = course.CreatedAt
            };

            return View(courseDTO);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Enroll(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var isEnrolled = await _courseRepository.IsStudentEnrolledAsync(user.Id, course.Id);
            if (isEnrolled)
            {
                TempData["Warning"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(MyCourses));
            }

            await _courseRepository.EnrollStudentAsync(user.Id, course.Id);

            TempData["Success"] = "Successfully enrolled in the course!";
            return RedirectToAction(nameof(MyCourses));
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var success = await _courseRepository.UnenrollStudentAsync(user.Id, id);
            if (success)
            {
                TempData["Success"] = "Successfully unenrolled from the course.";
            }
            else
            {
                TempData["Error"] = "Unable to unenroll from the course.";
            }

            return RedirectToAction(nameof(MyCourses));
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || course.TeacherId != user.Id)
                return Forbid();

            var courseDTO = new CourseDTO
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Content = course.Content,
                TeacherId = course.TeacherId
            };

            return View(courseDTO);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseDTO model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var course = await _courseRepository.GetByIdAsync(id);
                if (course == null)
                    return NotFound();

                var user = await _userManager.GetUserAsync(User);
                if (user == null || course.TeacherId != user.Id)
                    return Forbid();

                course.Title = model.Title;
                course.Description = model.Description;
                course.Content = model.Content;
                course.UpdatedAt = DateTime.UtcNow;

                await _courseRepository.UpdateAsync(course);
                await _courseRepository.SaveChangesAsync();

                TempData["Success"] = "Course updated successfully!";
                return RedirectToAction(nameof(TeacherCourses));
            }

            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || course.TeacherId != user.Id)
                return Forbid();

            await _courseRepository.DeleteAsync(id);
            await _courseRepository.SaveChangesAsync();

            TempData["Success"] = "Course deleted successfully!";
            return RedirectToAction(nameof(TeacherCourses));
        }
    }
}