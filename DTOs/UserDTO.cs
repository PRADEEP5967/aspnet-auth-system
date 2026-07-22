namespace AspNetAuthSystem.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}