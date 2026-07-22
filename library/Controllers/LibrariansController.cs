using library.DTO_s.Librarian;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            if(filterLibrarianDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterLibrarianDto.Id);
            }

            if(!string.IsNullOrWhiteSpace(filterLibrarianDto.Name))
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
                }) .ToListAsync(ct);

            return Ok(librarians);
        }

        [Authorize(Roles = "Librarian")]
        [HttpPut("UpdateLibrarian")]
        public async Task<IActionResult> UpdateLibrarian(long id,[FromBody] SaveLibrarianDto saveLibrarianDto, CancellationToken ct)
        {
            var librarian = await _dbContext.Librarians.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (librarian == null)
            {
                return NotFound($"Librarian with Id {id} not found.");
            }

            librarian.Name = saveLibrarianDto.Name;
            librarian.Email = saveLibrarianDto.Email;
            librarian.Phone = saveLibrarianDto.Phone;

            await _dbContext.SaveChangesAsync(ct);

            return NoContent();

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteLibrarian")]
        public async Task<IActionResult> DeleteLibrarian([FromQuery] long id, CancellationToken ct)
        {
            var librarian = await _dbContext.Librarians.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (librarian == null)
            {
                return NotFound($"Librarian with Id {id} not found.");
            }

            _dbContext.Librarians.Remove(librarian);
            await _dbContext.SaveChangesAsync(ct);

            return NoContent();

        }
    }
}
