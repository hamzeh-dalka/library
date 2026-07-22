using library.DTO_s.Login;
using library.DTO_s.Register;
using library.Models;
using library.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace library.Controllers 
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthController(LibraryDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterLibrarian")]
        public async Task<IActionResult> RegisterLibrarian([FromBody] RegisterLibrarianDto dto , CancellationToken ct)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName.ToLower() == dto.UserName.Trim().ToLower(), ct))
            {
                return BadRequest("Username already exists");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var user = CreateUser(dto.UserName, dto.Password, Role.Librarian);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(ct);

            var librarian = new Librarian
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                UserId = user.Id,
            };

            _dbContext.Librarians.Add(librarian);
            await _dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync();

            return Ok();

        }

        [HttpPost("RegisterStudent")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto dto, CancellationToken ct)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName.ToLower() == dto.UserName.Trim().ToLower(), ct))
            {
                return BadRequest("Username already exists");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var user = CreateUser(dto.UserName, dto.Password, Role.Student);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(ct);

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                UserId = user.Id,
                Faculty = dto.Faculty,
                MajorName = dto.MajorName
            };

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync();

            return Ok();

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken ct)
        {

            if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginDto.UserName.Trim().ToLower(), ct);

            if (user == null)
            {
                return BadRequest("Invalid username or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword))
            {
                return BadRequest("Invalid username or password");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });

        }

        private string GenerateJwtToken(User user)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private User CreateUser(string userName, string password, Role role)
        {
            return new User
            {
                UserName = userName,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
