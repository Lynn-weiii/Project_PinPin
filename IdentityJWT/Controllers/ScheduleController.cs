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

        //GET:api/schedule/GetMySchedules
        [HttpGet("GetMySchedules")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetMySchedules(int user_id)
        {
            try
            {
                List<ScheduleDTO> scheduleDTOs = await _context.Schedules
                    .AsNoTracking()
                    .Where(schedule => schedule.UserId == user_id)
                    .Select(schedule => new ScheduleDTO
                    {
                        Id = schedule.Id,
                        Name = schedule.Name,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        CreatedAt = schedule.CreatedAt,
                        UserId = schedule.UserId,
                    }).ToListAsync();

                if (scheduleDTOs.Count == 0)
                {
                    return NotFound("your schedule not found");
                }

                return Ok(scheduleDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //GET:api/schedule/GetRelatedSchedules
        [HttpGet("GetRelatedSchedules")]
        public async Task<ActionResult<Dictionary<int, string>>> GetRelatedSchedules(int user_id)
        {
            try
            {
                Dictionary<int, string> scheduleDictionary = await _context.ScheduleGroups
                    .Where(group => group.UserId == user_id)
                    .Include(group => group.Schedule)
                    .ToDictionaryAsync(group => group.ScheduleId, group => group.Schedule.Name);

                if (scheduleDictionary.Count == 0) return NotFound("Not found about your schedles");

                return Ok(scheduleDictionary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}