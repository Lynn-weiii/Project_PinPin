using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PinPinServer.Services
{
    public class AuthGetuserId
    {
        public async Task<ActionResult<int>> PinGetUserId(ClaimsPrincipal user)
        {
            if (user == null)
            {
                return new NotFoundResult();
            }

            // 获取名为 "nameidentifier" 的声明值，并尝试解析为整数
            var nameIdentifierClaim = user.Claims
                    .FirstOrDefault(c => c.Type.Contains("nameidentifier"));
            if (nameIdentifierClaim == null || !int.TryParse(nameIdentifierClaim.Value, out int userId))
            {
                return new NotFoundResult(); // 未找到有效的用户ID声明
            }

            // 模拟异步操作，例如等待一段时间或调用异步方法
            await Task.Delay(1000);

            // 返回用户ID
            return new ActionResult<int>(userId);
        }

    }
}

