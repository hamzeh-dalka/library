using library.Models.Enum;

namespace library.DTO_s
{
    public class FilterBorrowDto
    {
        public long? Id { get; set; }
        public DateTime? BorrowDate { get; set; }
        public BorrowStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
