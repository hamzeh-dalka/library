namespace library.DTO_s.Book
{
    public class BookDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int PublishedYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

    }
}
