namespace library.DTO_s.Book
{
    public class SaveBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int PublishedYear { get; set; }
        public long? CategoryId { get; set; }

    }
}
