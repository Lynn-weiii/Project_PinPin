using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private PinPinContext _context;
        private AuthGetuserId _getUserId;

        public userController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        //POST:api/user/GetAllUser
        [HttpPost("GetAllUser")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUser()
        {
            return Ok(await _context.Users.Select(user => new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Birthday = user.Birthday,
                Phone = user.Phone,
                Gender = user.Gender,
            }).ToListAsync());
        }

        //POST:api/user/GetUser
        [HttpPost("GetUser")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUser([FromForm] int? id, [FromForm] string? name)
        {
            if (id == null && String.IsNullOrEmpty(name)) return BadRequest("Either id or name must be provided");

            List<User> users = [];

            if (id != null)
            {
                User? id_user = await _context.Users.FirstOrDefaultAsync(o => o.Id == id);
                if (id_user != null) users.Add(id_user);
            }

            if (!String.IsNullOrEmpty(name))
            {
                List<User> name_users = await _context.Users.Where(o => o.Name.Contains(name)).ToListAsync();
                users.AddRange(name_users);
            }

            if (users.Count == 0) return NotFound("No users found");

            List<UserDTO> userDTOs = users.Select(user => new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Birthday = user.Birthday,
                Phone = user.Phone,
                Gender = user.Gender,
            }).ToList();

            return Ok(userDTOs);
        }


        //GET:api/user/GetUserIdName
        [HttpGet("GetUserIdName")]
        [Authorize]
        public async Task<ActionResult> GetUserIdName()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");
            var user = await _context.Users.FirstAsync(user => user.Id == userID.Value);

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
            });
        }
    }
}
