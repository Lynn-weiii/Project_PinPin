using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PinPinServer.Controllers
{
    //僅有admin權限可使用 [Authorize(Roles = "Admin")]
    //僅有User權限可使用 [Authorize(Roles = "User")]
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
        internal string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email)
            };

            //新增角色 0_管理員 1_一般用戶
            string roleName = user.Role switch
            {
                0 => "Admin",
                1 => "User",
                _ => throw new ArgumentException("Invalid role")
            };
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

    }
}
