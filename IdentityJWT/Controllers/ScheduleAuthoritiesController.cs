using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleAuthoritiesController : ControllerBase
    {
        PinPinContext _context;
        AuthGetuserId _getUserId;
        public ScheduleAuthoritiesController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        // GET: api/ScheduleAuthorities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleAuthority>>> GetScheduleAuthorities()
        {
            return await _context.ScheduleAuthorities.ToListAsync();
        }

        // GET: api/ScheduleAuthorities/{schedule_id}
        [HttpGet("{schedule_id}")]
        public async Task<ActionResult<ScheduleAuthority>> GetScheduleAuthority(int schedule_id)
        {
            int userID = _getUserId.PinGetUserId(User).Value;

            var scheduleAuthorities = await _context.ScheduleAuthorities
                .Where(sa => sa.ScheduleId == schedule_id && sa.UserId != userID)
                .Include(sa => sa.AuthorityCategory)
                .Include(sa => sa.User)
                .GroupBy(sa => new { sa.UserId, sa.User.Name })
                .Select(g => new ScheduleAuthorityDTO
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.Name,
                    ScheduleId = schedule_id,
                    AuthorityCategoryIds = g.Select(sa => sa.AuthorityCategoryId).Distinct().ToList(),
                }).ToListAsync();

            if (!scheduleAuthorities.Any())
            {
                return NoContent(); // Return 204 No Content if no data is available
            }

            return Ok(scheduleAuthorities); // Return 200 OK with the data
        }

        // PUT: api/ScheduleAuthorities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduleAuthority(int id, ScheduleAuthority scheduleAuthority)
        {
            if (id != scheduleAuthority.Id)
            {
                return BadRequest();
            }

            _context.Entry(scheduleAuthority).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleAuthorityExists(id))
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

        // POST: api/ScheduleAuthorities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ScheduleAuthority>> PostScheduleAuthority(ScheduleAuthority scheduleAuthority)
        {
            _context.ScheduleAuthorities.Add(scheduleAuthority);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetScheduleAuthority", new { id = scheduleAuthority.Id }, scheduleAuthority);
        }

        // DELETE: api/ScheduleAuthorities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleAuthority(int id)
        {
            var scheduleAuthority = await _context.ScheduleAuthorities.FindAsync(id);
            if (scheduleAuthority == null)
            {
                return NotFound();
            }

            _context.ScheduleAuthorities.Remove(scheduleAuthority);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleAuthorityExists(int id)
        {
            return _context.ScheduleAuthorities.Any(e => e.Id == id);
        }
    }
}
