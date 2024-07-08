using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PinPinServer.Services
{
    public class AuthGetuserId
    {
        public ActionResult<int> PinGetUserId(ClaimsPrincipal user)
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

            // 返回用户ID
            return new ActionResult<int>(userId);
        }

    }
}

