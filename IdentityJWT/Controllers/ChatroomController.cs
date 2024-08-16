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
        public async Task<ActionResult<List<ChatRoomDTO>>> GetChatRoom(int scheduleId)
        {
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
                        ScheduleId = scheduleId,
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
        public async Task<ActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //驗證傳入模型是否正確
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList()
                );

                return BadRequest(new { Error = errors });
            }

            //檢查有無在此行程表
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == dto.ScheduleId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            try
            {
                ChatroomChat chatroomChat = new ChatroomChat
                {
                    UserId = userID.Value,
                    ScheduleId = dto.ScheduleId,
                    Message = dto.Message,
                };
                await _context.ChatroomChats.AddAsync(chatroomChat);
                await _context.SaveChangesAsync();

                chatroomChat = await _context.ChatroomChats.Include(c => c.User).FirstAsync(c => c.Id == chatroomChat.Id);

                ChatRoomDTO newDto = new ChatRoomDTO
                {
                    Id = chatroomChat.Id,
                    UserId = chatroomChat.UserId,
                    UserName = chatroomChat.User.Name,
                    CreatedAt = chatroomChat.CreatedAt,
                    Message = chatroomChat.Message,
                    IsFocus = chatroomChat.IsFocus,
                    ScheduleId = chatroomChat.ScheduleId,
                };

                await _hubContext.Clients.Group($"Group_{dto.ScheduleId}").SendAsync("ReceiveMessage", newDto);
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
