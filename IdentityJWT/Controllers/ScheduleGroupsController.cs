using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;

namespace PinPinTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleGroupsController : ControllerBase
    {
        private PinPinContext _context;

        public ScheduleGroupsController(PinPinContext context)
        {
            _context = context;
        }

        //GET:api/ScheduleGroups
        [HttpGet]
        public async Task<ActionResult<Dictionary<int, string>>> GetScheduleGroups(int schedule_id)
        {
            return await _context.ScheduleGroups
                .Where(group => group.ScheduleId == schedule_id)
                .Include(group => group.User)
                .ToDictionaryAsync(user => user.UserId, user => user.User.Name);
        }
    }
}
