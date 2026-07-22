using Microsoft.AspNetCore.Identity;

namespace AspNetAuthSystem.Models
{
    public class Role : IdentityRole<int>
    {
        public Role() { }

        public Role(string roleName) : base(roleName)
        {
        }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}