using System.ComponentModel.DataAnnotations;

namespace AspNetAuthSystem.DTOs
{
    public class CourseDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course description is required")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Content { get; set; }

        public int TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}