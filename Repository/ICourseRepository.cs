using AspNetAuthSystem.Models;

namespace AspNetAuthSystem.Repository
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId);
        Task<IEnumerable<Course>> GetStudentCoursesAsync(int studentId);
        Task<Course?> GetCourseWithStudentsAsync(int courseId);
        Task EnrollStudentAsync(int studentId, int courseId);
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
        Task<bool> UnenrollStudentAsync(int studentId, int courseId);
    }
}