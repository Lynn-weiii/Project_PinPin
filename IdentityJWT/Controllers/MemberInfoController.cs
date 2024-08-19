using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using System.Security.Claims;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberInfoController : ControllerBase
    {
        private readonly PinPinContext _context;
        public MemberInfoController(PinPinContext context)
        {
            _context = context;
        }


        //GET:api/MemberInfo/SearchMemberInfo
        [Authorize]
        [HttpGet("SearchMemberInfo")]
        public async Task<ActionResult<User>> SearchMemberInfo()
        {
            //取出token中email的值
            string userEmail = User.Claims.First(x => x.Type == ClaimTypes.Email).Value;

            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            // 將圖檔轉換為Base64編碼
            string photoBase64 = null;
            if (user.Photo != null)
            {
                photoBase64 = user.Photo;
            }


            UserDTO userDto = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Birthday = user.Birthday,
                Gender = user.Gender,
                PhotoBase64 = photoBase64
            };
            // 回傳 UserDTO
            return Ok(userDto);
        }

        //PUT:api/MemberInfo/{email}
        [Authorize]
        [HttpPut("{email}")]
        public async Task<string> UpdateUser(string email, [FromBody] EditMemberInfoDTO userDto)
        {
            if (email != userDto.Email)
            {
                return "修改紀錄失敗";
            }

            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return "修改紀錄失敗";
            }
            else
            {
                user.Name = userDto.Name;
                user.Phone = userDto.Phone;
                user.Birthday = userDto.Birthday;
                user.Gender = userDto.Gender;
                user.Photo = userDto.Photo;
            }

            // 保存變更
            await _context.SaveChangesAsync();

            return "修改成功!";
        }
    }
}
