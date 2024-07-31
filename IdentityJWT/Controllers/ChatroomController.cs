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
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PinPinServer.Controllers
{
    [EnableCors("PinPinPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatroomController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatroomController> _logger;

        public ChatroomController(PinPinContext context, IConfiguration configuration, ILogger<ChatroomController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        //取得user所有群組，以及群組所有成員
        //GET:api/Chatroom/GetGroupMembers/{userId}
        [HttpGet("GetGroupMembers/{userId}")]
        public async IAsyncEnumerable<ScheduleGroupsDTO> GetGroupMembers(int userId)
        {
            await foreach (var group in _context.ScheduleGroups
                .Where(g => g.UserId == userId && g.LeftDate == null)
                .AsNoTracking()
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
                               UserName = sg.User.Name,
                               isHoster = sg.IsHoster
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
                var query = HttpContext.Request.Query;
                if (!query.TryGetValue("token", out var token))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Token missing", CancellationToken.None);
                    return;
                }

                var principal = ValidateToken(token);
                if (principal == null)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
                    return;
                }

                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
                WebSockets.TryAdd(userId, webSocket);  // 使用 userId 作为 key
                _logger.LogInformation($"WebSocket connection established for user ID: {userId}");
                await ProcessWebSocket(webSocket, userId);

            }
            else
            {
                _logger.LogWarning("Received a non-WebSocket request");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        //定義ProcessWebSocket函式
        private async Task ProcessWebSocket(WebSocket webSocket, int userId)
        {
            var buffer = new byte[1024 * 4];
            var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            try
            {
                while (!res.CloseStatus.HasValue)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, res.Count);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    ChatroomChat? receivedMsg = JsonSerializer.Deserialize<ChatroomChat>(json, options);
                    if (receivedMsg != null)
                    {
                        receivedMsg.UserId = userId;
                        receivedMsg.CreatedAt = DateTime.Now;

                        _context.ChatroomChats.Add(receivedMsg);
                        await _context.SaveChangesAsync();

                        Broadcast(receivedMsg);
                    }
                    res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocket processing");
            }
            finally
            {
                await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
                WebSockets.TryRemove(userId, out _);
            }
        }



        //定義Broadcase函式
        private void Broadcast(ChatroomChat message)
        {
            //平行運算
            Parallel.ForEach(WebSockets.Values, async (webSocket) =>
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        string msgJson = JsonSerializer.Serialize(message);
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgJson)), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error broadcasting message");
                    }
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
