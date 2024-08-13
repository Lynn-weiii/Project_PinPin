using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;

namespace PinPinServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly PinPinContext _context;

        public ChatHub(PinPinContext context)
        {
            _context = context;
        }

        /// <summary>
        /// groupId等於scheduleId
        /// </summary>
        /// <param name="groupId"></param>
        public async Task JoinGroup(int groupId, int userId)
        {
            //判斷是否屬於此群組
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == groupId && sg.UserId == userId);
            if (!isInSchedule) await Clients.Caller.SendAsync("JoinGroupFailed", "You do not have permission to join this group.");

            await Groups.AddToGroupAsync(groupId.ToString(), $"Group_{groupId}");
            await Clients.Caller.SendAsync("JoinGroupSuccess", $"Successfully joined Group_{groupId}");
        }

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }
    }
}
