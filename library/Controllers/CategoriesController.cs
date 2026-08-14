using library.DTO_s.Category;
using library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace library.Controllers
{
    [Route("api/Categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public CategoriesController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Librarian,Student")]
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories([FromQuery] FilterCategoryDto filterCategoryDto, CancellationToken ct)
        {
            var query = _dbContext.Categories.AsNoTracking().AsQueryable();

            if (filterCategoryDto.Id.HasValue)
            {
                query = query.Where(x => x.Id == filterCategoryDto.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterCategoryDto.Name))
            {
                query = query.Where(x => x.Name.Contains(filterCategoryDto.Name));
            }

            var categories = await query
                .OrderBy(x => x.Name)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(categories);
        }

        [Authorize(Roles = "Librarian")]
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory([FromBody] SaveCategoryDto saveCategoryDto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(saveCategoryDto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var nameExists = await _dbContext.Categories.AnyAsync(x =>
                x.Name.ToLower() == saveCategoryDto.Name.Trim().ToLower(), ct);

            if (nameExists)
            {
                return BadRequest($"Category with name '{saveCategoryDto.Name}' already exists.");
            }

            var category = new Category()
            {
                Name = saveCategoryDto.Name.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync(ct);

            return Ok();
        }

        //[Authorize(Roles = "Librarian")]
        //[HttpPut("UpdateCategory")]
        //public async Task<IActionResult> UpdateCategory(long id, [FromBody] SaveCategoryDto saveCategoryDto, CancellationToken ct)
        //{
        //    var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);

        //    if (category == null)
        //    {
        //        return NotFound($"Category with Id {id} not found.");
        //    }

        //    category.Name = saveCategoryDto.Name;

        //    await _dbContext.SaveChangesAsync(ct);

        //    return NoContent();

        //}

        [Authorize(Roles = "Librarian")]
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory([FromQuery] long id, CancellationToken ct)
        {

            var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (category == null)
            {
                return NotFound($"Category with Id {id} not found.");
            }

            var hasBooks = await _dbContext.Books.AnyAsync(x => x.CategoryId == id, ct);

            if (hasBooks)
            {
                return BadRequest($"Category with Id {id} cannot be deleted because it has associated books.");
            }

            _dbContext.Categories.Remove(category);
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
