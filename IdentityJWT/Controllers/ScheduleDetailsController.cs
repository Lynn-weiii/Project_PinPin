using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleDetailsController : ControllerBase
    {
        PinPinContext _context;
        AuthGetuserId _getUserId;
        public ScheduleDetailsController(PinPinContext context, AuthGetuserId getUserId)
        {
            _context = context;
            _getUserId = getUserId;
        }

        // GET: api/ScheduleDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDetail>>> GetScheduleDetails()
        {
            return await _context.ScheduleDetails.ToListAsync();
        }

        // GET: api/ScheduleDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleDetail>> GetScheduleDetail(int scheduleId)
        {
            int jwtUserId = _getUserId.PinGetUserId(User).Value;
            if (jwtUserId == null)
            {
                return Unauthorized(new { message = "請先登入會員" });
            }
            var scheduleDetail = await _context.ScheduleDetails.FindAsync(scheduleId);

            if (scheduleDetail == null)
            {
                return NotFound();
            }




            return scheduleDetail;
        }

        // PUT: api/ScheduleDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduleDetail(int id, ScheduleDetail scheduleDetail)
        {
            if (id != scheduleDetail.Id)
            {
                return BadRequest();
            }

            _context.Entry(scheduleDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ScheduleDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ScheduleDetail>> PostScheduleDetail(ScheduleDetail scheduleDetail)
        {
            _context.ScheduleDetails.Add(scheduleDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetScheduleDetail", new { id = scheduleDetail.Id }, scheduleDetail);
        }

        // DELETE: api/ScheduleDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleDetail(int id)
        {
            var scheduleDetail = await _context.ScheduleDetails.FindAsync(id);
            if (scheduleDetail == null)
            {
                return NotFound();
            }

            _context.ScheduleDetails.Remove(scheduleDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleDetailExists(int id)
        {
            return _context.ScheduleDetails.Any(e => e.Id == id);
        }
    }
}
