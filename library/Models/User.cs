using library.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace library.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    public class User
    {
        public long Id { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string HashedPassword { get; set; } = string.Empty;
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}