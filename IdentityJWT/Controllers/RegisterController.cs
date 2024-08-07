using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly PinPinContext _context;
        public RegisterController(PinPinContext context)
        {
            _context = context;
        }

        //POST:api/Register
        [HttpPost]
        public async Task<string> Register([FromForm] UserDTO userDTO)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);

            var existingPhone = userDTO.Phone != null ? await _context.Users.FirstOrDefaultAsync(u => u.Phone == userDTO.Phone) : null;
            //var existingPhone = await _context.Users.FirstOrDefaultAsync(p => p.Phone == userDTO.Phone);
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

            //圖檔
            string photoBase64 = null;
            if (userDTO.Photo != null && userDTO.Photo.Length > 0)
            {

                if (userDTO.Photo.Length > 2 * 1024 * 1024)
                {
                    return "圖片大小不能超過2MB。";
                }

                using (var ms = new MemoryStream())
                {
                    await userDTO.Photo.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    photoBase64 = Convert.ToBase64String(fileBytes);
                }
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 創建User物件
                    User user = new User
                    {
                        Name = userDTO.Name,
                        PasswordHash = passwordHash,
                        Email = userDTO.Email,
                        Phone = userDTO.Phone,
                        Birthday = userDTO.Birthday,
                        Gender = userDTO.Gender,
                        Photo = photoBase64,
                        CreatedAt = DateTime.Now,
                        Role = 1
                    };

                    // 將User物件加入資料庫上下文中
                    _context.Users.Add(user);

                    // 執行資料庫儲存異動，將User實體寫入資料庫
                    await _context.SaveChangesAsync();

                    
                    if (userDTO.favor != null && userDTO.favor.Length > 0)
                    {
                        foreach (var favorCategoryId in userDTO.favor)
                        {
                            UserFavor userFavor = new UserFavor
                            {
                                UserId = user.Id,
                                FavorCategoryId = favorCategoryId
                            };

                            _context.UserFavors.Add(userFavor);
                        }
                        await _context.SaveChangesAsync();
                    }

                    //建立預設的願望清單
                    Wishlist defaultWishlist = new Wishlist
                    {
                        UserId = user.Id,
                        Name = "我的願望清單"
                    };

                    _context.Wishlists.Add(defaultWishlist);
                    await _context.SaveChangesAsync();

                    //建立預設的願望清單分類
                    var defaultCategories = new List<LocationCategory>
                    {
                        new LocationCategory { WishlistId = defaultWishlist.Id, Name = "住宿", Color = "#FF5733" },
                        new LocationCategory { WishlistId = defaultWishlist.Id, Name = "景點", Color = "#33FF57" },
                        new LocationCategory { WishlistId = defaultWishlist.Id, Name = "餐廳", Color = "#3357FF" }
                    };

                    _context.LocationCategories.AddRange(defaultCategories);
                    await _context.SaveChangesAsync();

                    // 提交事務
                    transaction.Commit();

                    // 返回成功訊息，包含用戶編號
                    return $"註冊成功!會員編號:{user.Id}";
                }
                catch (Exception ex)
                {
                    // 如果發生異常，回滾事務
                    transaction.Rollback();
                    // 返回錯誤訊息或適當的異常處理
                    //return $"註冊失敗: {ex.Message}";
                    Console.WriteLine(ex.ToString());
                    throw;

                }
            }
        }

        private bool ValidatePassword(string password)
        {
            const int minLength = 8;
            const int maxLength = 16;
            bool hasLetter = password.Any(char.IsLetter);
            bool hasNumber = password.Any(char.IsDigit);

            return password.Length >= minLength && password.Length <= maxLength && hasLetter && hasNumber;
        }
    }
}
