using library.DTO_s.Book;
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
        private readonly Library3DbContext _dbContext;

        public BooksController(Library3DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks([FromQuery] FilterBookDto filterBookDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var query = _dbContext.Books
                .Include(x => x.Category)
                .AsNoTracking()
                .AsQueryable();

            if(filterBookDto.Id.HasValue)
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

            var books = await query
                .OrderBy(x => x.Title)
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
            var userId = GetCurrentUserId();

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

            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync(ct);

            return Ok();

        }

        [Authorize(Roles = "Librarian")]
        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> UpdateBook(long id, [FromBody] SaveBookDto saveBookDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            book.Title = saveBookDto.Title;
            book.Author = saveBookDto.Author;
            book.TotalCopies = saveBookDto.TotalCopies;
            book.AvailableCopies = saveBookDto.AvailableCopies;
            book.PublishedYear = saveBookDto.PublishedYear;
            book.CategoryId = saveBookDto.CategoryId;

            await _dbContext.SaveChangesAsync(ct);
            return NoContent();

        }

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(long id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
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
    }
}
