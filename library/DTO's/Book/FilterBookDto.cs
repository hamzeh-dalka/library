namespace library.DTO_s.Book
{
    public class FilterBookDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public int? PublishedYear { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
