using library.DTO_s;
using library.DTO_s.Book;
using library.DTO_s.Borrow;
using library.Models;
using library.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace library.Controllers
{
    [Route("api/Borrows")]
    [ApiController]
    public class BorrowsController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        public BorrowsController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Librarian")]
        [HttpGet("GetAllBorrows")]
        public async Task<IActionResult> GetAllBorrows([FromQuery] FilterBorrowDto filterBorrowDto, CancellationToken ct)
        {
            var pageNumber = filterBorrowDto.PageNumber < 1 ? 1 : filterBorrowDto.PageNumber;
            var pageSize = filterBorrowDto.PageSize < 1
                ? DefaultPageSize
                : Math.Min(filterBorrowDto.PageSize, MaxPageSize);

            var query = _dbContext.Borrows
                .Include(x => x.Student)
                .Include(x => x.Book)
                .AsNoTracking()
                .AsQueryable();

            if (filterBorrowDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterBorrowDto.Id);
            }

            if (filterBorrowDto.BorrowDate.HasValue)
            {
                var startOfDay = filterBorrowDto.BorrowDate.Value.Date;
                var endOfDay = startOfDay.AddDays(1);

                query = query.Where(x => x.BorrowDate >= startOfDay && x.BorrowDate < endOfDay);
            }

            if (filterBorrowDto.Status.HasValue)
            {
                query = query.Where(x => x.Status == filterBorrowDto.Status);
            }

            var borrows = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BorrowDto
                {
                    Id = x.Id,
                    BorrowDate = x.BorrowDate,
                    DueDate = x.DueDate,
                    ReturnDate = x.ReturnDate,
                    Status = x.Status,
                    StudentId = x.StudentId,
                    StudentName = x.Student.Name,
                    BookId = x.BookId,
                    BookTitle = x.Book.Title,
                    BookAuthor = x.Book.Author
                })
                .ToListAsync(ct);

            return Ok(borrows);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("GetBorrowsForStudent")]
        public async Task<IActionResult> GetBorrowsForStudent(CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var studentId = await _dbContext.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            if (studentId == 0)
            {
                return NotFound("Student profile not found for this user.");
            }

            var borrows = await _dbContext.Borrows
                .Where(x => x.StudentId == studentId)
                .AsNoTracking()
                .Include(x => x.Book)
                .ThenInclude(b => b.Category)
                .Select(x => new BorrowDto
                {
                    Id = x.Id,
                    BorrowDate = x.BorrowDate,
                    DueDate = x.DueDate,
                    ReturnDate = x.ReturnDate,
                    Status = x.Status,
                    StudentId = x.StudentId,
                    BookId = x.BookId,
                    BookTitle = x.Book.Title,
                    BookAuthor = x.Book.Author
                })
                .ToListAsync(ct);

            return Ok(borrows);
        }

        [Authorize(Roles = "Student")]
        [HttpPost("CreateBorrow")]
        public async Task<IActionResult> CreateBorrow([FromBody] SaveBorrowDto saveBorrowDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var studentId = await _dbContext.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            if (studentId == 0)
            {
                return NotFound("Student profile not found for this user.");
            }

            var borrowDate = DateTime.UtcNow;

            if (saveBorrowDto.DueDate <= borrowDate)
            {
                return BadRequest("Due date must be after the borrow date.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == saveBorrowDto.BookId, ct);

            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.AvailableCopies <= 0)
            {
                return BadRequest("No available copies of this book.");
            }

            var alreadyBorrowed = await _dbContext.Borrows.AnyAsync(b =>
                b.StudentId == studentId &&
                b.BookId == saveBorrowDto.BookId &&
                b.Status != BorrowStatus.Returned, ct);

            if (alreadyBorrowed)
            {
                return BadRequest("You already have an active borrow for this book.");
            }

            var borrow = new Borrow()
            {
                BorrowDate = borrowDate,
                DueDate = saveBorrowDto.DueDate,
                ReturnDate = null,
                Status = BorrowStatus.Borrowed,
                StudentId = studentId,
                BookId = saveBorrowDto.BookId
            };

            book.AvailableCopies -= 1;

            _dbContext.Borrows.Add(borrow);
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Ok();
        }

        [Authorize(Roles = "Student")]
        [HttpPatch("ReturnBook/{id}")]
        public async Task<IActionResult> ReturnBook(long id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var studentId = await _dbContext.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            var borrow = await _dbContext.Borrows
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId, ct);

            if (borrow == null)
            {
                return NotFound("Borrow record not found.");
            }

            if (borrow.Status == BorrowStatus.Returned)
            {
                return BadRequest("Book has already been returned.");
            }

            borrow.ReturnDate = DateTime.UtcNow;
            borrow.Status = BorrowStatus.Returned;

            if (borrow.Book != null)
            {
                borrow.Book.AvailableCopies = Math.Min(borrow.Book.AvailableCopies + 1, borrow.Book.TotalCopies);
            }

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Ok(new { Message = "Book returned" });
        }

        [Authorize(Roles = "Student")]
        [HttpPatch("ExtendDueDate/{id}")]
        public async Task<IActionResult> ExtendDueDate(long id, [FromBody] ExtendBorrowDto extendBorrowDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var studentId = await _dbContext.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            var borrow = await _dbContext.Borrows
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId, ct);

            if (borrow == null)
            {
                return NotFound("Borrow record not found.");
            }

            if (borrow.Status == BorrowStatus.Returned)
            {
                return BadRequest("Cannot extend due date for a returned book.");
            }

            if (extendBorrowDto.NewDueDate <= borrow.DueDate)
            {
                return BadRequest("New due date must be later than the current due date.");
            }

            borrow.DueDate = extendBorrowDto.NewDueDate;

            if (borrow.Status == BorrowStatus.Overdue)
            {
                borrow.Status = BorrowStatus.Borrowed;
            }

            await _dbContext.SaveChangesAsync(ct);

            return Ok(new { Message = "Due date extended" });
        }

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteBorrow/{id}")]
        public async Task<IActionResult> DeleteBorrow(long id, CancellationToken ct)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            var borrow = await _dbContext.Borrows
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (borrow == null)
            {
                return NotFound("Borrow record not found.");
            }

            if (borrow.Status != BorrowStatus.Returned && borrow.Book != null)
            {
                borrow.Book.AvailableCopies = Math.Min(borrow.Book.AvailableCopies + 1, borrow.Book.TotalCopies);
            }

            _dbContext.Borrows.Remove(borrow);
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

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