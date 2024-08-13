using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [EnableCors("PinPinPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatroomController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;
        private readonly IHubContext _hubContext;

        public ChatroomController(PinPinContext context, IHubContext hubContext, AuthGetuserId getUserId)
        {
            _context = context;
            _hubContext = hubContext;
            _getUserId = getUserId;
        }
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(int schedlueId, string message)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無在此行程表
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == schedlueId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            await _hubContext.Clients.Group($"Group_{schedlueId}").SendAsync("ReceiveMessage", userID, message);
            return Ok("Message sent successfully.");
        }

    }
}
