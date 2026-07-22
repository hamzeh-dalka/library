using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Librarian
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;

        [ForeignKey("User")]
        public long? UserId { get; set; }
        public User? User { get; set; }
    }
}
