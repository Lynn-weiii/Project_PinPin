using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using PinPinServer.Models;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatroomController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly IConfiguration _configuration;

        public ChatroomController(PinPinContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //取得user所有群組，以及群組所有成員
        //GET:api/Chatroom/GetGroupMembers/{userId}
        [HttpGet("GetGroupMembers/{userId}")]
        public async IAsyncEnumerable<ScheduleGroupsDTO> GetGroupMembers(int userId)
        {
            await foreach (var group in _context.ScheduleGroups
                .Where(g => g.UserId == userId && g.LeftDate == null)
                .Select(g => new ScheduleGroupsDTO
                {
                    Id = g.Id,
                    ScheduleId = g.Id,
                    UserId = g.UserId,
                    Members = g.Schedule.ScheduleGroups
                           .Where(sg => sg.LeftDate == null) // 過濾出尚未離開的成員
                           .Select(sg => new GroupMemberDTO
                           {
                               UserId = sg.UserId,
                               UserName = sg.User.Name
                           }).ToList()
                })
                .AsAsyncEnumerable())
            {
                yield return group;
            }
        }


        //紀錄所有連線的人
        static ConcurrentDictionary<int, WebSocket> WebSockets = new ConcurrentDictionary<int, WebSocket>();

        //有問題，待修改
        //GET:api/Chatroom/GetUserConnect
        [HttpGet("GetUserConnect")]
        public async Task GetUserConnect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                WebSockets.TryAdd(webSocket.GetHashCode(), webSocket);  //將連接進來的使用者加到WebSockets集合(ConcurrentDictionary)
                await ProcessWebSocket(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        //定義ProcessWebSocket函式
        private async Task ProcessWebSocket(WebSocket webSocket)
        {
            //處理身分驗證
            var query = HttpContext.Request.Query;
            if (!query.TryGetValue("token", out var token))
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Token missing", CancellationToken.None);
                return;
            }

            // Validate the token and extract the user ID
            var principal = ValidateToken(token);
            if (principal == null)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
                return;
            }

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);

            //處理訊息
            var buffer = new byte[1024 * 4]; //建立一個1024k大小的RAM空間，用來存放要傳送的資料

            //將接收到資料塞進buffer中，不做取消的處理
            var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!res.CloseStatus.HasValue)
            {
                string json = Encoding.UTF8.GetString(buffer, 0, res.Count);
                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                ChatroomChat? receivedMsg = JsonSerializer.Deserialize<ChatroomChat>(json, options);
                if (receivedMsg != null)
                {
                    receivedMsg.UserId = userId;
                    receivedMsg.CreatedAt = DateTime.Now;

                    _context.ChatroomChats.Add(receivedMsg);
                    await _context.SaveChangesAsync();

                    Broadcast(receivedMsg); //接收到的資料傳給Broadcase自訂函式，在此函式中廣播給所有連線的使用者
                }
                res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            //websocket關閉
            await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            //從WebSockets集合(ConcurrentDictionary)移除離線使用者
            WebSockets.TryRemove(webSocket.GetHashCode(), out var removed);
        }


        //定義Broadcase函式
        private void Broadcast(ChatroomChat message)
        {
            //平行運算
            Parallel.ForEach(WebSockets.Values, async (webSocket) =>
            {
                if (webSocket.State == WebSocketState.Open && webSocket.GetHashCode() != message.UserId)
                {
                    string msgJson = JsonSerializer.Serialize(message);
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(msgJson), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
        }

        //定義ValidateToken函式
        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!);
            try
            {
                var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                return claims;
            }
            catch
            {
                return null;
            }
        }
    }
}
