using library.Models;
using library.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace library.DTO_s.Borrow
{
    public class BorrowDto
    {
        public long Id { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowStatus Status { get; set; }
        public long? StudentId { get; set; }
        public string? StudentName { get; set; }
        public long? BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
    }
}
