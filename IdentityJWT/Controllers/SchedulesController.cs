using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using PinPinServer.Services;


namespace PinPinServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        PinPinContext _context;
        AuthGetuserId _getUserId;
        private int userID;
        public SchedulesController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        // GET: api/Schedules
        [HttpGet]
        public async Task<IActionResult> GetUserAllSchedule()
        {
            IEnumerable<ScheduleDTO> schedules = Enumerable.Empty<ScheduleDTO>();
            try
            {
                int userID = _getUserId.PinGetUserId(User).Value;
                schedules = await _context.Schedules
                        .Where(s => s.UserId == userID)
                        .Join(
                            _context.Users,
                            sch => sch.UserId,
                            usr => usr.Id,
                            (sch, usr) => new ScheduleDTO
                            {
                                Id = sch.Id,
                                UserId = sch.UserId,
                                Name = sch.Name,
                                StartTime = sch.StartTime,
                                EndTime = sch.EndTime,
                                CreatedAt = sch.CreatedAt,
                                UserName = usr.Name
                            }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                throw new Exception("伺服器發生錯誤，請稍後再試");
            }

            if (schedules == null || !schedules.Any())
            {
                Console.WriteLine("查無使用者相關紀錄");
            }

            return Ok(schedules);
        }

        //Get:api/Schedules/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetUserSpecifiedSch(string name)
        {
            IEnumerable<ScheduleDTO> schedules = Enumerable.Empty<ScheduleDTO>();
            try
            {
                int userID = _getUserId.PinGetUserId(User).Value;
                schedules = await _context.Schedules
                    .Where(s => s.UserId == userID && s.Name.Contains(name))
                    .Join(
                        _context.Users,
                        sch => sch.UserId,
                        usr => usr.Id,
                        (sch, usr) => new ScheduleDTO
                        {
                            Id = sch.Id,
                            UserId = sch.UserId,
                            Name = sch.Name,
                            StartTime = sch.StartTime,
                            EndTime = sch.EndTime,
                            CreatedAt = sch.CreatedAt,
                            UserName = usr.Name
                        })
                    .ToListAsync();

                if (schedules == null || !schedules.Any())
                {
                    return NotFound(new { message = "未找到匹配的行程" });
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                // 可以记录异常日志，以便后续排查
                Console.WriteLine($"處理請求時發生錯誤: {ex.Message}");
                return StatusCode(500, new { message = "處理請求時發生錯誤，請稍後再試。" });
            }
        }

        // PUT: api/Schedules/{id}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchedule(int id, ScheduleDTO schDTO)
        {

            int userID = _getUserId.PinGetUserId(User).Value;
            Schedule sch = await _context.Schedules.FindAsync(id);

            if (sch == null)
            {
                return NotFound();
            }

            sch.Id = schDTO.Id;
            sch.Name = schDTO.Name;
            sch.StartTime = schDTO.StartTime;
            sch.EndTime = schDTO.EndTime;
            sch.UserId = userID;
            sch.CreatedAt = schDTO.CreatedAt;
            _context.Entry(sch).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("行程修改成功!");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
                {
                    return BadRequest("系統發生錯誤");
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Schedules
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EditScheduleDTO>> PostSchedule([FromBody] EditScheduleDTO editschDTO)
        {
            try
            {
                int userID = _getUserId.PinGetUserId(User).Value;
                Schedule newschDTO = new Schedule
                {
                    Name = editschDTO.Name,
                    StartTime = editschDTO.StartTime,
                    EndTime = editschDTO.EndTime,
                    CreatedAt = DateTime.Now,
                    UserId = userID
                };


                _context.Schedules.Add(newschDTO);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        // DELETE: api/Schedules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            int userID = _getUserId.PinGetUserId(User).Value;
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound(); // 404 Not Found if schedule with the given id is not found
            }
            try
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                return Ok("行程刪除"); // 200 OK if deletion is successful
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return StatusCode(500, "刪除失敗!"); // 500 Internal Server Error if deletion fails
            }
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
