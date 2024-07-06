using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PinPinServer.DTO;
using PinPinServer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PinPinServer.Controllers
{

    [EnableCors("PinPinPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        private readonly PinPinContext _context;

        public AuthController(PinPinContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        //POST:api/Auth/Register

        [HttpPost("Register")]
        public async Task<string> Register(UserDTO userDTO)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);

            var existingPhone = await _context.Users.FirstOrDefaultAsync(p => p.Phone == userDTO.Phone);
            if (existingUser != null)
            {
                //return BadRequest("該電子郵件已經被註冊");
                return "該電子郵件已經被註冊";
            }

            if (existingPhone != null)
            {
                //return BadRequest("該電子郵件已經被註冊");
                return "該電話號碼已經被註冊";
            }

            if (userDTO.Password != userDTO.PasswordConfirm)
            {
                //return BadRequest( "請再次確認密碼!");
                return "請再次確認密碼";
            }

            if (!ValidatePassword(userDTO.Password))
            {
                //return BadRequest("密碼必須為8-16個字符，且包含英文及數字。");
                return "密碼必須為8-16個字符，且包含英文及數字";
            }


            string passwordHash
                   = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);


            User user = new User
            {

                Name = userDTO.Name,
                PasswordHash = passwordHash,
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                Birthday = userDTO.Birthday,
                Gender = userDTO.Gender,
                Photo = userDTO.Photo,
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return $"註冊成功!會員編號:{user.Id}";
        }

        private bool ValidatePassword(string password)
        {
            const int minLength = 8;
            const int maxLength = 16;
            bool hasLetter = password.Any(char.IsLetter);
            bool hasNumber = password.Any(char.IsDigit);

            return password.Length >= minLength && password.Length <= maxLength && hasLetter && hasNumber;
        }

        //POST:api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(LoginDTO request)
        {
            if (request == null)
            {
                return BadRequest("請求資料無效");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);


            if (user == null)
            {
                return BadRequest("帳號錯誤");
            }


            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("密碼錯誤");
            }

            string token = CreateToken(user);
            //return Ok(new { Token = token });//直接傳送JSON格式
            return Ok(token);
        }

        //建立Token
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }



        //GET:api/Auth/SearchMemberInfo
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
            UserDTO userDto = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Birthday = user.Birthday,
                Gender = user.Gender,
                Photo = user.Photo
            };
            // 回傳 UserDTO
            return Ok(userDto);
        }

        //PUT:api/Auth/{email}
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
                //user.Photo = userDto.Photo;
            }


            // 更新用戶資料


            // 保存變更
            await _context.SaveChangesAsync();

            return "修改成功!";
        }

        //[Authorize]
        //[HttpPost("Logout")]
        //public IActionResult Logout()
        //{   
        //    return Ok("登出成功");
        //}
    }
}
