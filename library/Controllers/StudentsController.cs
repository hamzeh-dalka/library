using library.DTO_s.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> GetAllStudents([FromQuery] FilterStudentDto filterStudentDto , CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var query = _dbContext.Students.AsQueryable();

            if(filterStudentDto.Id.HasValue)
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

            if(!string.IsNullOrWhiteSpace(filterStudentDto.MajorName))
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
                }) .ToListAsync(ct);

            return Ok(students);

        }
        
        [Authorize(Roles = "Student")]
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent(long id ,[FromBody] SaveStudentDto saveStudentDto , CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.UserId == userId , ct);

            if (student == null)
            {
                return NotFound($"Student profile not found.");
            }

            student.Name = saveStudentDto.Name;
            student.Faculty = saveStudentDto.Faculty;
            student.MajorName = saveStudentDto.MajorName;
            student.Email = saveStudentDto.Email;
            student.Phone = saveStudentDto.Phone;

            await _dbContext.SaveChangesAsync(ct);

            return NoContent();

        }

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteStudent")]
        public async Task<IActionResult> DeleteStudent([FromQuery] long id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var student = await _dbContext.Students.FindAsync(id, ct);

            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            _dbContext.Students.Remove(student);
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
