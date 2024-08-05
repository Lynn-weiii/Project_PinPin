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

        // POST: api/ScheduleAuthorities/Modified
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Modified")]
        public async Task<IActionResult> Modified([FromBody] List<ScheduleAuthorityDTO> saDTOs)
        {
            if (saDTOs == null || !saDTOs.Any())
            {
                return BadRequest("Invalid data.");
            }

            bool hasChanges = false; // 标志用于跟踪是否有变化

            foreach (var saDTO in saDTOs)
            {
                if (saDTO.AuthorityCategoryIds == null || !saDTO.AuthorityCategoryIds.Any())
                {
                    return BadRequest("AuthorityCategoryIds cannot be empty.");
                }

                var existingAuthorities = await _context.ScheduleAuthorities
                    .Where(sa => sa.ScheduleId == saDTO.ScheduleId && sa.UserId == saDTO.UserId)
                    .ToListAsync();

                var existingAuthorityIds = existingAuthorities.Select(ea => ea.AuthorityCategoryId).ToHashSet();
                var newAuthorityIds = saDTO.AuthorityCategoryIds.ToHashSet();

                bool isSame = existingAuthorityIds.SetEquals(newAuthorityIds);

                // 如果权限未变化，跳过当前 saDTO
                if (isSame)
                {
                    continue;
                }

                // 权限有变化，标记有变化
                hasChanges = true;

                // 删除现有权限并添加新权限
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
                }).ToList();
                return Ok(resultDTOs);
            }

            // 如果没有任何变化，返回 OK
            if (!hasChanges)
            {
                return Ok(new { message = "No changes detected" });
            }

            return Ok(); // 有变化时的默认返回
        }
    }
}
