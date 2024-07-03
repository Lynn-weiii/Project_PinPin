using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityJWT
{
    public class GetuserId
    {
        public ActionResult<int> GetUserId(ClaimsPrincipal user)
        {
            var nameIdentityValue = user.Claims
                .Where(c => c.Type.Contains("nameidentifier"))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (nameIdentityValue != null)
            {
                // 如果 nameIdentityValue 不是空，输出并返回它
                Console.WriteLine(nameIdentityValue);
                if (int.TryParse(nameIdentityValue, out int userId))
                {
                    return new ActionResult<int>(userId);
                }
                else
                {
                    return new ActionResult<int>(-1); // 返回一个错误码，表示无法解析为整数
                }
            }
            else
            {
                // 如果没有找到匹配的声明，返回一个错误码，表示没有找到
                Console.WriteLine("找不到該用戶");
                return new ActionResult<int>(-1); // 返回一个错误码，表示没有找到
            }
        }

    }
}
