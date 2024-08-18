using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Hubs;
using PinPinServer.Models;
using PinPinServer.Models.DTO.Message;
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

        //GET:api/Chatroom
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
                    .OrderBy(cc=>cc.CreatedAt)
                    .Select(cc => new ChatRoomDTO
                    {
                        Id = cc.Id,
                        ScheduleId = scheduleId,
                        UserId = cc.UserId,
                        UserName = cc.User.Name,
                        CreatedAt = cc.CreatedAt,
                        Message = cc.Message,
                        IsFocus = cc.IsFocus,
                        IsEdited = cc.IsEdited,
                        LastEditedAt = cc.LastEditedAt,
                        IsDeleted = cc.IsDeleted,
                    }).ToListAsync();

                return Ok(dtos);
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        //POST:api/Chatroom/SendMessage
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage([FromBody] MessageDTO dto)
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

            //檢查是否有訊息
            if (string.IsNullOrEmpty(dto.Message)) return BadRequest("Message can't be empty.");

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
                    IsEdited = chatroomChat.IsEdited,
                    LastEditedAt = chatroomChat.LastEditedAt,
                    IsDeleted = chatroomChat.IsDeleted,
                };

                await _hubContext.Clients.Group($"Group_{dto.ScheduleId}").SendAsync("ReceiveMessage", newDto);
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //PUT:api/Chatroom
        [HttpPut]
        public async Task<ActionResult> PutChatMessage([FromBody] MessageDTO messageDto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            ChatroomChat? chatroomChat = await _context.ChatroomChats
                                .Include(cc=>cc.User)
                                .FirstOrDefaultAsync(cc => cc.Id == messageDto.Id);
            //檢查有無此訊息
            if (chatroomChat == null) return NotFound("Message not found");

            //檢查是否是這位使用者的訊息
            if (chatroomChat.UserId != userID.Value) return Forbid("You are not allowed to edit this message.");

            //檢查是否有訊息
            if (string.IsNullOrEmpty(chatroomChat.Message)) return BadRequest("Message can't be empty.");

            try
            {
                chatroomChat.Message = messageDto.Message;
                chatroomChat.IsEdited = true;
                chatroomChat.LastEditedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                ChatRoomDTO dto = new ChatRoomDTO
                {
                    Id = chatroomChat.Id,
                    UserId = userID.Value,
                    Message = chatroomChat.Message,
                    IsFocus = chatroomChat.IsFocus,
                    CreatedAt = chatroomChat.CreatedAt,
                    ScheduleId = chatroomChat.ScheduleId,
                    UserName = chatroomChat.User.Name,
                    IsEdited = chatroomChat.IsEdited,
                    LastEditedAt = chatroomChat.LastEditedAt,
                    IsDeleted = chatroomChat.IsDeleted
                };

                await _hubContext.Clients.Group($"Group_{dto.ScheduleId}").SendAsync("ReceiveUpdatedMessage", dto);
                return Ok();
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        //PUT:api/Chatroom
        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            ChatroomChat? chatroomChat = await _context.ChatroomChats.Include(cc=>cc.User).FirstOrDefaultAsync(cc=>cc.Id==messageId);

            //檢查有無此訊息
            if (chatroomChat == null) return NotFound("Message not found");

            //檢查有無權限
            bool isSelf = userID.Value == chatroomChat.UserId;
            bool hasAccess = await _context.ScheduleAuthorities
                .AnyAsync(sa => sa.UserId == userID && sa.ScheduleId == chatroomChat.ScheduleId && sa.AuthorityCategoryId == 4);
            if(!(isSelf || hasAccess)) return Forbid("You can't delete someone else's message.");
            try
            {
                chatroomChat.IsDeleted = true;
                chatroomChat.Message = "";
                chatroomChat.LastEditedAt=DateTime.Now;

                _context.ChatroomChats.Update(chatroomChat);
                await _context.SaveChangesAsync();

                ChatRoomDTO dto = new ChatRoomDTO
                {
                    Id = chatroomChat.Id,
                    UserId = userID.Value,
                    Message = chatroomChat.Message,
                    IsFocus = chatroomChat.IsFocus,
                    CreatedAt = chatroomChat.CreatedAt,
                    ScheduleId = chatroomChat.ScheduleId,
                    UserName = chatroomChat.User.Name,
                    IsEdited = chatroomChat.IsEdited,
                    LastEditedAt = chatroomChat.LastEditedAt,
                    IsDeleted = chatroomChat.IsDeleted
                };

                await _hubContext.Clients.Group($"Group_{dto.ScheduleId}").SendAsync("ReceiveDeleteMessage", dto);
                return Ok("Message deleted successfully");
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

    }
}
