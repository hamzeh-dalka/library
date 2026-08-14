using library.DTO_s.Librarian;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace library.Controllers
{
    [Route("api/Librarians")]
    [ApiController]
    public class LibrariansController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public LibrariansController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllLibrarians")]
        public async Task<IActionResult> GetAllLibrarians([FromQuery] FilterLibrarianDto filterLibrarianDto, CancellationToken ct)
        {
            var query = _dbContext.Librarians.AsQueryable();

            if (filterLibrarianDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterLibrarianDto.Id);
            }

            if (!string.IsNullOrWhiteSpace(filterLibrarianDto.Name))
            {
                query = query.Where(x => x.Name.Contains(filterLibrarianDto.Name));
            }

            var librarians = await query
                .OrderBy(x => x.Name)
                .Select(x => new LibrarianDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone
                }).ToListAsync(ct);

            return Ok(librarians);
        }

        [Authorize(Roles = "Librarian")]
        [HttpPut("UpdateLibrarian")]
        public async Task<IActionResult> UpdateLibrarian([FromBody] SaveLibrarianDto saveLibrarianDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var librarian = await _dbContext.Librarians.FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (librarian == null)
            {
                return NotFound($"Librarian with Id {userId} not found.");
            }

            if (string.IsNullOrWhiteSpace(saveLibrarianDto.Name) || string.IsNullOrWhiteSpace(saveLibrarianDto.Email))
            {
                return BadRequest("Name and Email are required.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(
            saveLibrarianDto.Email.Trim(),
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("Email format is invalid.");
            }

            var emailTaken = await _dbContext.Librarians.AnyAsync(x =>
                x.Id != librarian.Id &&
                x.Email.ToLower() == saveLibrarianDto.Email.Trim().ToLower(), ct);

            if (emailTaken)
            {
                return BadRequest("This email is already in use by another librarian.");
            }

            if (string.IsNullOrWhiteSpace(saveLibrarianDto.Phone))
            {
                return BadRequest("Phone is required.");
            }

            librarian.Name = saveLibrarianDto.Name.Trim();
            librarian.Email = saveLibrarianDto.Email.Trim();
            librarian.Phone = saveLibrarianDto.Phone.Trim();

            await _dbContext.SaveChangesAsync(ct);

            return NoContent();

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteLibrarian")]
        public async Task<IActionResult> DeleteLibrarian([FromQuery] long id, CancellationToken ct)
        {
            var librarian = await _dbContext.Librarians
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (librarian == null)
            {
                return NotFound($"Librarian with Id {id} not found.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            if (librarian.User != null)
            {
                _dbContext.Users.Remove(librarian.User);
            }

            _dbContext.Librarians.Remove(librarian);
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
