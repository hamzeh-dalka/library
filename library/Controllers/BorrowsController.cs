using library.DTO_s;
using library.DTO_s.Borrow;
using library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using library.Models.Enum;
using Microsoft.AspNetCore.Authorization;

namespace library.Controllers
{
    [Route("api/Borrows")]
    [ApiController]
    public class BorrowsController : ControllerBase
    {
        private readonly Library3DbContext _dbContext;

        public BorrowsController(Library3DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize(Roles = "librarian")]
        [HttpGet("GetAllBorrows")]
        public async Task<IActionResult> GetAllBorrows([FromQuery] FilterBorrowDto filterBorrowDto , CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var query = _dbContext.Borrows
                .Include(x => x.User)
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
                .Skip((filterBorrowDto.PageNumber - 1) * filterBorrowDto.PageSize)
                .Take(filterBorrowDto.PageSize)
                .Select(x => new BorrowDto
                {
                   Id = x.Id,
                    BorrowDate = x.BorrowDate,
                    DueDate = x.DueDate,
                    ReturnDate = x.ReturnDate,
                    Status = x.Status,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
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

            var borrow = new Borrow()
            {
                BorrowDate = saveBorrowDto.BorrowDate,
                DueDate = saveBorrowDto.DueDate,
                ReturnDate = saveBorrowDto.ReturnDate,
                Status = BorrowStatus.Borrowed,
                UserId = userId,
                BookId = saveBorrowDto.BookId
            };

            _dbContext.Borrows.Add(borrow);
            await _dbContext.SaveChangesAsync(ct);

            return Ok();
        }
        [Authorize(Roles = "Student")]
        [HttpPatch("ReturnBook/{id}")]
        public async Task<IActionResult> ReturnBook(long id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var borrow = await _dbContext.Borrows.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

            if (borrow == null)
            {
                return NotFound("Book not found.");
            }

            if (borrow.Status == BorrowStatus.Returned)
            {
                return BadRequest("Book has already been returned.");
            }

            borrow.ReturnDate = DateTime.Now;
            borrow.Status = BorrowStatus.Returned;

            await _dbContext.SaveChangesAsync(ct);

            return Ok(new { Message = "Book returned" });

        }
        
        [Authorize(Roles = "Student")]
        [HttpPatch("ExtendDueDate/{id}")]
        public async Task<IActionResult> ExtendDueDate(long id, [FromBody] ExtendBorrowDto extendBorrowDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var borrow = await _dbContext.Borrows.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

            if (borrow == null)
            {
                return NotFound("Borrow record not found.");
            }

            if (borrow.Status == BorrowStatus.Returned)
            {
                return BadRequest("Cannot extend due date for a returned book.");
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
            var userId = GetCurrentUserId();

            var borrow = await _dbContext.Borrows.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (borrow == null)
            {
                return NotFound("Borrow record not found.");
            }

            _dbContext.Borrows.Remove(borrow);
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
