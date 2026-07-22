using library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace library.DTO_s.Borrow
{
    public class SaveBorrowDto
    {
        [Required(ErrorMessage = "Borrow date is required.")]
        public DateTime BorrowDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public long? StudentId { get; set; }
        public long? BookId { get; set; }
    }
}
