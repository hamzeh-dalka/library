using library.DTO_s.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace library.Controllers
{
    [Route("api/Students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public StudentsController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Librarian")]
        [HttpGet("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents([FromQuery] FilterStudentDto filterStudentDto, CancellationToken ct)
        {
            var query = _dbContext.Students.AsQueryable();

            if (filterStudentDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterStudentDto.Id);
            }

            if (!string.IsNullOrEmpty(filterStudentDto.Name))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filterStudentDto.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterStudentDto.Faculty))
            {
                query = query.Where(x => x.Faculty.ToLower().Contains(filterStudentDto.Faculty.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filterStudentDto.MajorName))
            {
                query = query.Where(x => x.MajorName.ToLower().Contains(filterStudentDto.MajorName.ToLower()));
            }

            var students = await query
                .OrderBy(x => x.Id)
                .Select(x => new StudentDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Faculty = x.Faculty,
                    MajorName = x.MajorName,
                    Email = x.Email,
                    Phone = x.Phone
                }).ToListAsync(ct);

            return Ok(students);

        }

        [Authorize(Roles = "Student")]
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent([FromBody] SaveStudentDto saveStudentDto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (student == null)
            {
                return NotFound($"Student profile not found.");
            }

            if(string.IsNullOrWhiteSpace(saveStudentDto.Name) || string.IsNullOrWhiteSpace(saveStudentDto.Faculty) || string.IsNullOrWhiteSpace(saveStudentDto.MajorName) || string.IsNullOrWhiteSpace(saveStudentDto.Email) || string.IsNullOrWhiteSpace(saveStudentDto.Phone))
            {
                return BadRequest("All fields are required.");
            }

            if (!Regex.IsMatch(saveStudentDto.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("Email format is invalid.");
            }

            var emailTaken = await _dbContext.Students.AnyAsync(x =>
                x.Id != student.Id &&
                x.Email.ToLower() == saveStudentDto.Email.Trim().ToLower(), ct);

            if (emailTaken)
            {
                return BadRequest("This email is already in use by another student.");
            }

            student.Name = saveStudentDto.Name.Trim();
            student.Faculty = saveStudentDto.Faculty.Trim();
            student.MajorName = saveStudentDto.MajorName.Trim();
            student.Email = saveStudentDto.Email.Trim();
            student.Phone = saveStudentDto.Phone.Trim();

            await _dbContext.SaveChangesAsync(ct);

            return NoContent();

        }

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteStudent")]
        public async Task<IActionResult> DeleteStudent([FromQuery] long id, CancellationToken ct)
        {
            var student = await _dbContext.Students
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            if (student.User != null)
            {
                _dbContext.Users.Remove(student.User);
            }

            _dbContext.Students.Remove(student);
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
