namespace AspNetAuthSystem.Models
{
    public class StudentCourse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}