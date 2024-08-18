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

            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
                await Clients.Caller.SendAsync("JoinGroupSuccess", $"Successfully joined Group_{groupId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending test message: {ex.Message}");
            }
        }

        //測試用api
        //public async Task SendTestMessageToGroup(int groupId)
        //{
        //    await Clients.Group($"Group_{groupId}").SendAsync("ReceiveTestMessage", "This is a test message.");
        //}

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        }
    }
}
