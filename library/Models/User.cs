using library.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace library.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}