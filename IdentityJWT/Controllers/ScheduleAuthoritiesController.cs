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
        private readonly ILogger<ScheduleAuthoritiesController> _logger;
        PinPinContext _context;
        AuthGetuserId _getUserId;
        public ScheduleAuthoritiesController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        #region 讀取行程成員權限
        // GET: api/ScheduleAuthorities/{schedule_id}
        [HttpGet("{schedule_id}")]
        public async Task<ActionResult<ScheduleAuthority>> GetScheduleAuthority(int schedule_id)
        {
            int jwtuserid = _getUserId.PinGetUserId(User).Value;
            var groupMemberIds = await _context.ScheduleGroups
            .Where(sg => sg.ScheduleId == schedule_id && sg.LeftDate == null && sg.UserId != jwtuserid)
            .Select(sg => sg.UserId)
            .ToListAsync();

            if (groupMemberIds.Count == 0)
            {
                //return 204
                return NoContent();
            }
            else
            {
                try
                {
                    var filteredAuthorities = await _context.ScheduleAuthorities
                    .Where(sa => groupMemberIds.Contains(sa.UserId) && sa.ScheduleId == schedule_id)
                    .GroupBy(sa => new { sa.UserId, sa.User.Name, sa.ScheduleId })
                    .Select(g => new ScheduleAuthorityDTO
                    {
                        Id = g.First().Id,
                        UserName = g.Key.Name,
                        ScheduleId = g.Key.ScheduleId,
                        AuthorityCategoryIds = g.Select(sa => sa.AuthorityCategoryId).Distinct().ToList(),
                        UserId = g.Key.UserId,
                    }).ToListAsync();
                    return Ok(filteredAuthorities);
                    //return 200
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                    return StatusCode(500, ex.Message);
                }
            }
        }
        #endregion

        #region 修改權限
        // POST: api/ScheduleAuthorities/Modified
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Modified")]
        public async Task<IActionResult> Modified([FromBody] List<ScheduleAuthorityDTO> saDTOs)
        {
            if (saDTOs == null || !saDTOs.Any())
            {
                return BadRequest(new { message = "Invalid data." });
            }

            var messages = new List<string>();

            try
            {
                foreach (var saDTO in saDTOs)
                {
                    if (saDTO.AuthorityCategoryIds == null || !saDTO.AuthorityCategoryIds.Any())
                    {
                        return BadRequest(new { message = "Invalid data." });
                    }

                    var existingAuthorities = await _context.ScheduleAuthorities
                        .Where(sa => sa.ScheduleId == saDTO.ScheduleId && sa.UserId == saDTO.UserId)
                        .ToListAsync();

                    var existingAuthorityIds = existingAuthorities.Select(ea => ea.AuthorityCategoryId).ToHashSet();
                    var newAuthorityIds = saDTO.AuthorityCategoryIds.ToHashSet();
                    bool isSame = existingAuthorityIds.SetEquals(newAuthorityIds);

                    if (isSame)
                    {
                        continue;
                    }

                    var userName = await _context.Users
                        .Where(u => u.Id == saDTO.UserId)
                        .Select(u => u.Name)
                        .FirstOrDefaultAsync();

                    // Add a single message for this user
                    messages.Add($"{userName} 的權限已修改");

                    if (existingAuthorities.Any())
                    {
                        _context.ScheduleAuthorities.RemoveRange(existingAuthorities);
                    }

                    foreach (var authorityCategoryId in saDTO.AuthorityCategoryIds)
                    {
                        var adduserauthority = new ScheduleAuthority
                        {
                            ScheduleId = saDTO.ScheduleId,
                            UserId = saDTO.UserId,
                            AuthorityCategoryId = authorityCategoryId,
                        };

                        _context.ScheduleAuthorities.Add(adduserauthority);
                    }
                }

                await _context.SaveChangesAsync();

                if (messages.Count == 0)
                {
                    return Ok(new { message = "目前沒有成員權限變更，因此無需進行修改。" });
                }

                return Ok(new { message = string.Join("; ", messages) });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (ScheduleAuthority)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    //舊資料已經更新成新版本了
                    return Ok();
                }
                else
                {
                    var databaseValues = (ScheduleAuthority)databaseEntry.ToObject();
                    return StatusCode(409, new
                    {
                        message = "發生並發衝突，數據已被其他用戶修改。",
                        clientValues = clientValues,
                        databaseValues = databaseValues
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { message = "內部伺服器錯誤。", error = ex.Message });
            }
        }
        #endregion
    }
}