using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Services;

namespace PinPinTest.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleGroupsController : ControllerBase
    {
        private PinPinContext _context;
        private readonly AuthGetuserId _getUserId;

        public ScheduleGroupsController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        //GET:api/ScheduleGroups
        [HttpGet]
        public async Task<ActionResult<Dictionary<int, string>>> GetScheduleGroups(int schedule_id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");
            try
            {
                Dictionary<int, string> groupUsers = await _context.ScheduleGroups
                   .Where(group => group.ScheduleId == schedule_id)
                   .Include(group => group.User)
                   .ToDictionaryAsync(
                       user => user.UserId,
                       user => user.User.Name
                   );

                if (!groupUsers.ContainsKey(userID.Value)) return Forbid("You can't search not your group");

                return Ok(groupUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
