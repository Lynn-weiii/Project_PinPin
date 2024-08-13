using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Hubs;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [EnableCors("PinPinPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatroomController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatroomController(PinPinContext context, IHubContext<ChatHub> hubContext, AuthGetuserId getUserId)
        {
            _context = context;
            _hubContext = hubContext;
            _getUserId = getUserId;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetChatRoom(int scheduleId) {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無在此行程表
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == scheduleId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            try
            {
                List<ChatRoomDTO> dtos = await _context.ChatroomChats
                    .Where(cc => cc.ScheduleId == scheduleId)
                    .Include(cc => cc.User)
                    .Select(cc => new ChatRoomDTO
                    {
                        Id = cc.Id,
                        UserId = cc.UserId,
                        UserName = cc.User.Name,
                        CreatedAt = cc.CreatedAt,
                        Message = cc.Message,
                        IsFocus = cc.IsFocus,
                    }).ToListAsync();

                return Ok(dtos);
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(int scheduleId, string message)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無在此行程表
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == scheduleId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            await _hubContext.Clients.Group($"Group_{scheduleId}").SendAsync("ReceiveMessage", userID, message);
            return Ok("Message sent successfully.");
        }

    }
}
