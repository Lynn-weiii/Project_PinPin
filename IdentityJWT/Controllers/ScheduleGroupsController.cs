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

        //GET:api/ScheduleGroups
        [HttpGet]
        public async Task<ActionResult<Dictionary<int, string>>> GetScheduleGroups(int schedule_id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            try
            {
                Dictionary<int, string> groupUsers = await _context.ScheduleGroups
                   .Where(group => group.ScheduleId == schedule_id)
                   .Include(group => group.User)
                   .ToDictionaryAsync(
                       user => user.UserId,
                       user => user.User.Name
                   );

                if (!groupUsers.ContainsKey(userID.Value)) return Forbid("You can't search not your group");

                return Ok(groupUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //POST:api/ScheduleGroups/Invitemember
        [HttpPost("Invitemember")]
        public async Task<IActionResult> Invitemember([FromBody] InviteMemeberDTO inviteMemeberDTO)
        {

            int MemberId = _context.Users
                            .Where(u => u.Email == inviteMemeberDTO.Email)
                            .Select(u => u.Id)
                            .Distinct()
                            .FirstOrDefault();

            if (MemberId == 0) return BadRequest("無此會員");


            bool isGroupMember = _context.ScheduleGroups
                .Any(sg => sg.UserId == MemberId && sg.ScheduleId == inviteMemeberDTO.ScheduleId);
            if (isGroupMember) return BadRequest("會員已加入此群組");


            bool isHoster = _context.Schedules
                            .Any(s => s.Id == inviteMemeberDTO.ScheduleId && s.UserId == MemberId);
            if (isHoster) return BadRequest("主辦者不能加入群組");

            var newmember = new ScheduleGroup
            {
                Id = 0,
                ScheduleId = inviteMemeberDTO.ScheduleId,
                UserId = MemberId,
                IsHoster = false,
                JoinedDate = DateTime.Now,
                LeftDate = null
            };
            _context.ScheduleGroups.Add(newmember);
            await _context.SaveChangesAsync();
            var newmemberauthority = new ScheduleAuthority
            {
                Id = 0,
                ScheduleId = inviteMemeberDTO.ScheduleId,
                UserId = MemberId,
                AuthorityCategoryId = 1,
            };
            _context.ScheduleAuthorities.Add(newmemberauthority);
            await _context.SaveChangesAsync();

            return Ok(newmember);
        }

    }
}
