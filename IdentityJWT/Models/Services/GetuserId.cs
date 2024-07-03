using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace PinPinServer.Services
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
                Console.WriteLine(nameIdentityValue);
                if (int.TryParse(nameIdentityValue, out int userId))
                {
                    return new ActionResult<int>(userId);
                }
                else
                {
                    return new ActionResult<int>(-1);
                }
            }
            else
            {

                return new ActionResult<int>(-1);
            }
        }

    }
}

