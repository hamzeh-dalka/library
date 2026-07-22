using library.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Borrow
    {
        public long Id { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowStatus Status { get; set; }

        [ForeignKey("Student")]
        public long? StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("Book")]
        public long? BookId { get; set; }
        public Book? Book { get; set; }
        
    }
}
