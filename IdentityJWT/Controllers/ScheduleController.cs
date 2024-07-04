using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;

namespace PinPinTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class scheduleController : ControllerBase
    {
        private PinPinContext _context;

        public scheduleController(PinPinContext context)
        {
            _context = context;
        }

        //POST:api/schedule/GetAllschedule
        [HttpPost("GetAllschedule")]
        public async Task<IEnumerable<ScheduleDTO>> GetAllSchedule()
        {
            return await _context.Schedules.Select(schedule => new ScheduleDTO
            {
                Id = schedule.Id,
                Name = schedule.Name,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                CreatedAt = schedule.CreatedAt,
                UserId = schedule.UserId,
            }).ToListAsync();
        }
    }
}
