using System.ComponentModel.DataAnnotations;

namespace library.Models
{
    public class Category
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
