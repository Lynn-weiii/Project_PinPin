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
                    Id = schedule_id,
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

        // POST: api/ScheduleAuthorities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Modified")]
        public async Task<IActionResult> Modified([FromBody] ScheduleAuthorityDTO saDTO)
        {
            if (saDTO == null || !saDTO.AuthorityCategoryIds.Any())
            {
                return BadRequest("Invalid data.");
            }

            //修改的過程:刪掉原來資料>>>新增新的
            //檢查回傳的資料是不是需要修改
            var existingAuthorities = await _context.ScheduleAuthorities
                .Where(sa => sa.ScheduleId == saDTO.ScheduleId && sa.UserId == saDTO.UserId)
                .ToListAsync();
            bool allMatch = existingAuthorities.All(ea =>
                saDTO.AuthorityCategoryIds.Contains(ea.AuthorityCategoryId) &&
                existingAuthorities.Count(e => e.AuthorityCategoryId == ea.AuthorityCategoryId) == saDTO.AuthorityCategoryIds.Count(a => a == ea.AuthorityCategoryId));

            if (!allMatch)
            {
                if (existingAuthorities.Any())
                {
                    _context.ScheduleAuthorities.RemoveRange(existingAuthorities);
                }

                var addedAuthorities = new List<ScheduleAuthority>();
                foreach (var authorityCategoryId in saDTO.AuthorityCategoryIds)
                {
                    var adduserauthority = new ScheduleAuthority
                    {
                        ScheduleId = saDTO.ScheduleId,
                        UserId = saDTO.UserId,
                        AuthorityCategoryId = authorityCategoryId
                    };

                    _context.ScheduleAuthorities.Add(adduserauthority);
                    addedAuthorities.Add(adduserauthority);
                }

                await _context.SaveChangesAsync();
                var resultDTOs = addedAuthorities.Select(a => new ScheduleAuthorityDTO
                {
                    ScheduleId = a.ScheduleId,
                    UserId = a.UserId,
                    AuthorityCategoryIds = new List<int> { a.AuthorityCategoryId },
                    UserName = a.User.Name
                }).ToList();
                return Ok(resultDTOs);
            }
            return Ok();
        }
    }
}
