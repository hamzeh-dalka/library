using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Borrow
    {
        public long Id { get; set; }
        public DateTime BorrowDare { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        [ForeignKey("User")]
        public long? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("Book")]
        public long? BookId { get; set; }
        public Book? Book { get; set; }
        public enum Status
        {
            Borrowed,
            Returned,
            Late
        }
    }
}
