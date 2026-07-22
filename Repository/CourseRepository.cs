using AspNetAuthSystem.Data;
using AspNetAuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetAuthSystem.Repository
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Teacher)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetStudentCoursesAsync(int studentId)
        {
            return await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.Course!)
                .Include(c => c.Teacher)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseWithStudentsAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.StudentCourses)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task EnrollStudentAsync(int studentId, int courseId)
        {
            var enrollment = new StudentCourse
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };
            await _context.StudentCourses.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
        }

        public async Task<bool> UnenrollStudentAsync(int studentId, int courseId)
        {
            var enrollment = await _context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
            
            if (enrollment != null)
            {
                _context.StudentCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}