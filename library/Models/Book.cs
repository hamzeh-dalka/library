using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Book
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int PuplishedYear { get; set; }

        [ForeignKey("Category")]
        public long? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
