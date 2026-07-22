using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Book
    {
        public long Id { get; set; }


        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int PublishedYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Embedding { get; set; }

        [ForeignKey("Category")]
        public long? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
