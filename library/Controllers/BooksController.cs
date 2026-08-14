using library.DTO_s.Book;
using library.Enums;
using library.Interfaces;
using library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace library.Controllers
{
    [Route("api/Books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;
        private readonly IAIService _aiService;

        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        public BooksController(LibraryDbContext dbContext, IAIService aiService)
        {
            _dbContext = dbContext;
            _aiService = aiService;
        }

        [Authorize(Roles = "Librarian,Student")]
        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks([FromQuery] FilterBookDto filterBookDto, CancellationToken ct)
        {
            var pageNumber = filterBookDto.PageNumber < 1 ? 1 : filterBookDto.PageNumber;
            var pageSize = filterBookDto.PageSize < 1
                ? DefaultPageSize
                : Math.Min(filterBookDto.PageSize, MaxPageSize);

            var query = _dbContext.Books
                .Include(x => x.Category)
                .AsNoTracking()
                .AsQueryable();

            if (filterBookDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterBookDto.Id.Value);
            }

            if (!string.IsNullOrEmpty(filterBookDto.Author))
            {
                query = query.Where(x => x.Author.ToLower().Contains(filterBookDto.Author.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterBookDto.Title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(filterBookDto.Title.ToLower()));
            }

            if (filterBookDto.PublishedYear.HasValue)
            {
                query = query.Where(x => x.PublishedYear == filterBookDto.PublishedYear.Value);
            }

            if (filterBookDto.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filterBookDto.CategoryId.Value);
            }

            var books = await query
                .OrderBy(x => x.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Author = x.Author,
                    TotalCopies = x.TotalCopies,
                    AvailableCopies = x.AvailableCopies,
                    PublishedYear = x.PublishedYear,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(books);
        }

        [Authorize(Roles = "Librarian")]
        [HttpPost("AddBook")]
        public async Task<IActionResult> AddBook([FromBody] SaveBookDto saveBookDto, CancellationToken ct)
        {
            if (saveBookDto.TotalCopies < 0)
            {
                return BadRequest("TotalCopies cannot be negative.");
            }

            bool bookExists = await _dbContext.Books.AnyAsync(x =>
                x.Title.ToLower() == saveBookDto.Title.Trim().ToLower() &&
                x.Author.ToLower() == saveBookDto.Author.Trim().ToLower(), ct);

            if (bookExists)
            {
                return BadRequest("This book already exists.");
            }

            var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == saveBookDto.CategoryId, ct);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            var book = new Book()
            {
                Title = saveBookDto.Title,
                Author = saveBookDto.Author,
                TotalCopies = saveBookDto.TotalCopies,
                AvailableCopies = saveBookDto.TotalCopies,
                PublishedYear = saveBookDto.PublishedYear,
                CategoryId = saveBookDto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var textForEmbedding = $"{book.Title} {book.Author}";
                var vector = await _aiService.GenerateEmbeddingAsync(textForEmbedding);
                book.Embedding = System.Text.Json.JsonSerializer.Serialize(vector);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Embedding generation failed: {ex.Message}");
                book.Embedding = null;
            }

            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync(ct);

            return Ok();
        }

        [Authorize(Roles = "Librarian")]
        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> UpdateBook(long id, [FromBody] SaveBookDto saveBookDto, CancellationToken ct)
        {
            if (saveBookDto.TotalCopies < 0)
            {
                return BadRequest("TotalCopies cannot be negative.");
            }

            var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == saveBookDto.CategoryId, ct);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            var totalCopiesDelta = saveBookDto.TotalCopies - book.TotalCopies;
            var newAvailableCopies = book.AvailableCopies + totalCopiesDelta;
            book.AvailableCopies = Math.Clamp(newAvailableCopies, 0, saveBookDto.TotalCopies);

            var titleOrAuthorChanged =
                !string.Equals(book.Title, saveBookDto.Title, StringComparison.Ordinal) ||
                !string.Equals(book.Author, saveBookDto.Author, StringComparison.Ordinal);

            book.Title = saveBookDto.Title;
            book.Author = saveBookDto.Author;
            book.TotalCopies = saveBookDto.TotalCopies;
            book.PublishedYear = saveBookDto.PublishedYear;
            book.CategoryId = saveBookDto.CategoryId;

            if (titleOrAuthorChanged)
            {
                try
                {
                    var textForEmbedding = $"{book.Title} {book.Author}";
                    var vector = await _aiService.GenerateEmbeddingAsync(textForEmbedding);
                    book.Embedding = System.Text.Json.JsonSerializer.Serialize(vector);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Embedding generation failed: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync(ct);
            return NoContent();
        }

        [Authorize(Roles = "Student")]
        [HttpGet("GetRecommendedBooks")]
        public async Task<IActionResult> GetRecommendedBooks(CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var studentId = await _dbContext.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            if (studentId == 0)
            {
                return NotFound("Student profile not found.");
            }

            var lastBorrow = await _dbContext.Borrows
                .Where(b => b.StudentId == studentId)
                .OrderByDescending(b => b.BorrowDate)
                .Include(b => b.Book)
                .FirstOrDefaultAsync(ct);

            if (lastBorrow?.Book == null || string.IsNullOrEmpty(lastBorrow.Book.Embedding))
            {
                var latestBooks = await _dbContext.Books
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(x => new BookDto
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Author = x.Author,
                        TotalCopies = x.TotalCopies,
                        AvailableCopies = x.AvailableCopies,
                        PublishedYear = x.PublishedYear,
                        CategoryId = x.CategoryId,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(ct);

                return Ok(latestBooks);
            }

            var targetVector = System.Text.Json.JsonSerializer.Deserialize<float[]>(lastBorrow.Book.Embedding);

            var allBooks = await _dbContext.Books
                .Where(b => b.Id != lastBorrow.BookId && b.Embedding != null)
                .ToListAsync(ct);

            var recommendedBooks = allBooks
                .Select(b => new
                {
                    Book = b,
                    Similarity = CalculateCosineSimilarity(
                        targetVector,
                        System.Text.Json.JsonSerializer.Deserialize<float[]>(b.Embedding!))
                })
                .OrderByDescending(x => x.Similarity)
                .Take(5)
                .Select(x => new BookDto
                {
                    Id = x.Book.Id,
                    Title = x.Book.Title,
                    Author = x.Book.Author,
                    TotalCopies = x.Book.TotalCopies,
                    AvailableCopies = x.Book.AvailableCopies,
                    PublishedYear = x.Book.PublishedYear,
                    CategoryId = x.Book.CategoryId,
                    CreatedAt = x.Book.CreatedAt
                })
                .ToList();

            return Ok(recommendedBooks);
        }

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(long id, CancellationToken ct)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            var hasActiveBorrows = await _dbContext.Borrows.AnyAsync(b =>
                b.BookId == id && b.Status != BorrowStatus.Returned, ct);

            if (hasActiveBorrows)
            {
                return BadRequest("Cannot delete this book while it has active borrows.");
            }

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync(ct);

            return NoContent();
        }

        private long GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id))
            {
                throw new UnauthorizedAccessException();
            }
            return long.Parse(id);
        }

        private double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1 == null || vector2 == null || vector1.Length != vector2.Length)
                return 0;

            double dotProduct = 0, magnitude1 = 0, magnitude2 = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }
            if (magnitude1 == 0 || magnitude2 == 0) return 0;
            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }
    }
}