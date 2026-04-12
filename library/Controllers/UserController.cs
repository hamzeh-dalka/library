using AutoMapper;
using AutoMapper.QueryableExtensions;
using library.DTO_s.UserDto_s;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace library.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ColLibraryDbContext _dbContext;
        //private object userDto;
        private IMapper _mapper;

        public UserController(ColLibraryDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public UserController(IMapper mapper) 
        {
            _mapper = mapper;
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers(string? name)
        {
            try
            {
                var users = from user in _dbContext.Users
                            where user.Name == name
                            select new UserDto
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Email = user.Email
                            };
                return Ok(users.ToList());
            }
            catch (Exception ex) 
            { 
                return StatusCode(500,"Something went wrong");
            }
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(long id) 
        {
            var user = await _dbContext.Users
                        .Where(u => u.Id == id)
                        .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync();

                return Ok(user);       
        }
    }
}
