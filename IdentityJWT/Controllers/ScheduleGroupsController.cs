using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
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

        #region 待確認有沒有要使用 2024/8/8
        //GET:api/ScheduleGroups
        [HttpGet("GetScheduleGroups{schedule_id}")]
        public async Task<ActionResult<Dictionary<int, string>>> GetScheduleGroups(int schedule_id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            try
            {
                Dictionary<int, string> groupUsers = await _context.ScheduleGroups
                .Where(group => group.ScheduleId == schedule_id && group.LeftDate == null)
                .Include(group => group.User)
                .ToDictionaryAsync(
                    group => group.UserId,
                    group => group.User.Name
                );

                if (!groupUsers.ContainsKey(userID.Value)) return Forbid("You can't search not your group");

                return Ok(groupUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        #endregion

        #region 搜尋行程群組的成員
        //GET:api/ScheduleGroups/GetGropsMembers
        [HttpGet("GetGroupsMembers/{gschedule_id}")]
        public async Task<ActionResult<List<GroupDTO>>> GetGroupsMembers(int gschedule_id)
        {
            int? jwtuserID = _getUserId.PinGetUserId(User).Value;
            if (jwtuserID == null || jwtuserID == 0)
            {
                return BadRequest();
            }

            try
            {
                int hostID = await _context.Schedules
                    .Where(s => s.Id == gschedule_id)
                    .Select(s => s.UserId)
                    .FirstOrDefaultAsync();

                // 检查当前用户是否具有移除成员的权限 (AuthorityCategoryId == 3)
                bool userCanRemove = await _context.ScheduleAuthorities
                    .AnyAsync(sa => sa.ScheduleId == gschedule_id && sa.AuthorityCategoryId == 3 && sa.UserId == jwtuserID);

                // 获取所有群组成员
                var groupDTOs = await _context.ScheduleGroups
                    .Where(g => g.ScheduleId == gschedule_id && g.LeftDate == null)
                    .Include(g => g.User)
                    .ThenInclude(u => u.ScheduleAuthorities)
                    .Select(g => new GroupDTO
                    {
                        UserId = g.UserId,
                        UserName = g.User.Name,
                        UserPhoto = g.User.Photo,
                        AuthorityIds = g.User.ScheduleAuthorities
                            .Where(a => a.ScheduleId == gschedule_id)
                            .Select(a => a.AuthorityCategoryId)
                            .ToList(),
                        Canremoveid = g.UserId != hostID && g.UserId != jwtuserID,
                        Removeauth = userCanRemove,
                        IsHost = g.UserId == hostID
                    }).ToListAsync();


                var finalGroupDTOs = groupDTOs
                    .Select(member => new GroupDTO
                    {
                        UserId = member.UserId,
                        UserName = member.UserName,
                        UserPhoto = member.UserPhoto,
                        AuthorityIds = member.AuthorityIds,
                        Canremoveid = member.Canremoveid,
                        Removeauth = member.Removeauth,
                        IsHost = member.IsHost
                    })
                    .ToList();

                return Ok(finalGroupDTOs); // 返回最终处理后的 DTO 列表
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        #endregion

        #region 邀請成員
        //POST:api/ScheduleGroups/Invitemember
        [HttpPost("Invitemember")]
        public async Task<IActionResult> Invitemember([FromBody] InviteMemeberDTO inviteMemeberDTO)
        {
            int amount = await _context.ScheduleGroups.Where(s => s.ScheduleId == inviteMemeberDTO.ScheduleId && s.LeftDate == null).CountAsync();
            if (amount >= 6)
            {
                return BadRequest(new { message = "群組人數已滿" });
            }

            try
            {
                var member = await _context.Users
                .Where(u => u.Email == inviteMemeberDTO.Email)
                .Select(u => new { u.Id })
                .FirstOrDefaultAsync();

                if (member == null)
                {
                    return BadRequest(new { message = "無此會員" });
                }

                bool isHoster = await _context.ScheduleGroups
                    .AnyAsync(s => s.ScheduleId == inviteMemeberDTO.ScheduleId && s.UserId == member.Id && s.IsHoster == true);
                if (isHoster)
                {
                    return BadRequest(new { message = "主辦者不需加入群組" });
                }

                bool isGroupMember = await _context.ScheduleGroups
                    .AnyAsync(sg => sg.UserId == member.Id && sg.ScheduleId == inviteMemeberDTO.ScheduleId && sg.LeftDate == null);
                if (isGroupMember)
                {
                    return BadRequest(new { message = "會員已加入此群組" });
                }

                var wasmember = await _context.ScheduleGroups.FirstOrDefaultAsync(s => s.ScheduleId == inviteMemeberDTO.ScheduleId && s.UserId == member.Id && s.LeftDate.HasValue);

                if (wasmember != null)
                {
                    wasmember.LeftDate = null;
                    _context.ScheduleGroups.Update(wasmember);
                    await _context.SaveChangesAsync();

                    var updateMemberAuthority = new ScheduleAuthority
                    {
                        ScheduleId = inviteMemeberDTO.ScheduleId,
                        UserId = member.Id,
                        AuthorityCategoryId = inviteMemeberDTO.AuthorityCategoryId
                    };
                    _context.ScheduleAuthorities.Add(updateMemberAuthority);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "已新增成員進入群組!" });
                }
                else
                {
                    var newmember = new ScheduleGroup
                    {
                        Id = 0,
                        ScheduleId = inviteMemeberDTO.ScheduleId,
                        UserId = member.Id,
                        IsHoster = false,
                        JoinedDate = DateTime.Now,
                        LeftDate = null
                    };
                    _context.ScheduleGroups.Add(newmember);
                    var newmemberauthority = new ScheduleAuthority
                    {
                        Id = 0,
                        ScheduleId = inviteMemeberDTO.ScheduleId,
                        UserId = member.Id,
                        AuthorityCategoryId = inviteMemeberDTO.AuthorityCategoryId
                    };
                    _context.ScheduleAuthorities.Add(newmemberauthority);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "已新增成員進入群組!" });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry != null)
                {
                    return Ok(new { message = "已新增成員進入群組!" });
                }
                else
                {
                    var updatedData = (ScheduleGroup)databaseEntry.ToObject();
                    _context.Entry(updatedData).OriginalValues.SetValues(entry.OriginalValues);
                    await _context.SaveChangesAsync();
                    return StatusCode(409);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "伺服器錯誤", detail = ex.Message });
            }
        }
        #endregion

        #region 剔除成員/離開群組
        //PUT:api/ScheduleGroups/Memberexit
        [HttpPut("Memberexit")]
        public async Task<ActionResult> Memberexit([FromBody] exitmemberDTO exitDTO)
        {
            int jwtuserId = _getUserId.PinGetUserId(User).Value;
            int userId = exitDTO.UserID;
            if(userId == 0)
            {
                userId = jwtuserId;
            }
            var ismember = await _context.ScheduleGroups
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ScheduleId == exitDTO.ScheduleID && s.LeftDate == null);

            if (ismember == null)
            {
                return BadRequest("無此成員");
            }

            try
            {
                ismember.LeftDate = DateTime.Now;
                _context.ScheduleGroups.Update(ismember);
                await _context.SaveChangesAsync();

                var removememberauth = await _context.ScheduleAuthorities
                    .Where(sa => sa.UserId == userId && sa.ScheduleId == exitDTO.ScheduleID)
                    .ToListAsync();

                if (removememberauth.Any())
                {
                    _context.ScheduleAuthorities.RemoveRange(removememberauth);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { Message = "成員已退出" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseEntry = entry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    return Ok();
                }
                else
                {
                    var updatedData = (ScheduleGroup)databaseEntry.ToObject();
                    _context.Entry(updatedData).OriginalValues.SetValues(entry.OriginalValues);
                    await _context.SaveChangesAsync();
                    return StatusCode(409, new { message = "伺服器錯誤", detail = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "伺服器錯誤", detail = ex.Message });
            }
        }
        #endregion

        private bool GroupMemberExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}