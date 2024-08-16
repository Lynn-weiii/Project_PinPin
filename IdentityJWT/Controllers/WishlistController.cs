using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;
using PinPinServer.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PinPinServer.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly PinPinContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        AuthGetuserId _getUserId;

        public WishlistController(PinPinContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, AuthGetuserId getuserId)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _getUserId = getuserId;
        }

        //取得user所有願望清單
        //GET:api/Wishlist/GetAllWishlist/{userId?}
        [HttpGet("GetAllWishlist/{userId?}")]
        public async Task<ActionResult<IEnumerable<WishlistDTO>>> GetAllWishlist(int? userId)
        {
            if (userId == null)
            {
                int jwtuserId = _getUserId.PinGetUserId(User).Value;
                if (jwtuserId == null)
                {
                    return Unauthorized(new { message = "請先登入會員" });
                }
                userId = jwtuserId;
            }
            //查找user有沒有願望清單
            var wishlists = await _context.Wishlists
            .Include(w => w.LocationCategories)
            .Include(w => w.WishlistDetails)
            .Where(w => w.UserId == userId)
            .ToListAsync();

            if (wishlists == null || !wishlists.Any())
            {
                return NotFound(new { message = "您尚未建立願望清單" });
            }

            var result = wishlists.Select(w => new WishlistDTO
            {
                Id = w.Id,
                UserId = w.UserId,
                Name = w.Name,
                LocationCategories = w.LocationCategories.Select(lc => new LocationCategoryDTO
                {
                    Id = lc.Id,
                    WishlistId = lc.WishlistId,
                    Name = lc.Name,
                    Color = lc.Color,
                    Icon = lc.Icon
                }).ToList(),
                WishlistDetails = w.WishlistDetails.Select(d => new WishlistDetailDTO
                {
                    WishlistId = d.WishlistId,
                    Name = d.Name,
                    LocationLng = d.LocationLng,
                    LocationLat = d.LocationLat,
                    GooglePlaceId = d.GooglePlaceId,
                    LocationCategoryId = d.LocationCategoryId,
                    CreatedAt = d.CreatedAt
                }).ToList() // 新增這一行
            }).ToList();

            return Ok(result);
        }


        //加入願望清單
        //POST:api/Wishlist/AddtoWishlistDetail
        [HttpPost("AddtoWishlistDetail")]
        public async Task<IActionResult> AddtoWishlistDetail([FromBody] WishlistDetailDTO wishlistDetailDTO)
        {
            if (wishlistDetailDTO == null)
            {
                return BadRequest(new { message = "請再重新試一次" });
            }

            //判斷是否已存在清單
            var existingDetail = await _context.WishlistDetails
       .FirstOrDefaultAsync(wd => wd.WishlistId == wishlistDetailDTO.WishlistId && wd.GooglePlaceId == wishlistDetailDTO.GooglePlaceId);

            if (existingDetail != null)
            {
                return BadRequest(new { message = "已有景點在願望清單內" });
            }

            var wishlistDetail = new WishlistDetail
            {
                WishlistId = wishlistDetailDTO.WishlistId,
                LocationLng = wishlistDetailDTO.LocationLng,
                LocationLat = wishlistDetailDTO.LocationLat,
                GooglePlaceId = wishlistDetailDTO.GooglePlaceId,
                Name = wishlistDetailDTO.Name,
                LocationCategoryId = wishlistDetailDTO.LocationCategoryId,
                CreatedAt = wishlistDetailDTO.CreatedAt
            };

            _context.WishlistDetails.Add(wishlistDetail);
            await _context.SaveChangesAsync();

            return Ok();
        }

        //取得願望清單細節
        // GET: api/Wishlist/GetWishlistDetails
        [HttpGet("GetWishlistDetails")]
        public async Task<IActionResult> GetSpotDetails(string placeId, string photoReference)
        {
            if (string.IsNullOrEmpty(placeId) || string.IsNullOrEmpty(photoReference))
            {
                return BadRequest("placeId and photoReference parameters are required.");
            }

            var apiKey = _configuration["GoogleMaps:ApiKey"];
            var client = _httpClientFactory.CreateClient();

            // Get Photo URL
            var photoUrl = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference={photoReference}&key={apiKey}";

            // Get Details
            var detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&language=zh-TW&key={apiKey}";
            var response = await client.GetAsync(detailsUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var detailsResult = await response.Content.ReadAsStringAsync();

            // Combine Photo URL and Details in one response
            return Ok(new
            {
                photoUrl,
                details = Newtonsoft.Json.JsonConvert.DeserializeObject(detailsResult)
            });
        }

        //新增願望清單OK
        //POST:api/Wishlist/CreateWishlist
        [HttpPost("CreateWishlist")]
        public async Task<ActionResult<Wishlist>> CreateWishlist(Wishlist wishlist)
        {
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                id = wishlist.Id,
                userId = wishlist.UserId,
                name = wishlist.Name
            });
            //return Content("新增成功!");
        }


        //修改願望清單OK
        //PUT:api/Wishlist/UpdateWishlist/{id}
        [HttpPut("UpdateWishlist/{id}")]
        public async Task<IActionResult> UpdateWishlist(int id, [FromBody] WishlistUpdateDTO updateDTO)
        {
            // 取得userId
            var user = await _context.Users.FindAsync(updateDTO.UserId);
            if (user == null)
            {
                return BadRequest("無此用戶");
            }

            // 取得願望清單
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist == null || wishlist.UserId != updateDTO.UserId)
            {
                return BadRequest("無此願望清單或無修改權限");
            }

            // 更新願望清單
            wishlist.Name = updateDTO.Name;

            // 儲存修改
            _context.Entry(wishlist).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WishlistExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Content("願望清單修改成功！");
        }

        private bool WishlistExists(int id)
        {
            return _context.Wishlists.Any(e => e.Id == id);
        }

        //刪除願望清單Ok
        // DELETE:api/Wishlist/DeleteWishlist/{id}
        [HttpDelete("DeleteWishlist/{id}")]
        public async Task<IActionResult> DeleteWishlist(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist == null)
            {
                return NotFound();
            }
            // 刪除相關的wishlistDetail
            var relatedLocationCategory = _context.LocationCategories
                .Where(category => category.WishlistId == id);
            _context.LocationCategories.RemoveRange(relatedLocationCategory);

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();

            return Content("願望清單刪除成功!");
        }


        //新增locationCategory OK
        //POST:api/Wishlist/CreateLocationCategory
        [HttpPost("CreateLocationCategory")]
        public async Task<ActionResult<LocationCategory>> CreateLocationCategory(LocationCategory locationCategory)
        {
            if (locationCategory == null || string.IsNullOrWhiteSpace(locationCategory.Name) || !Validator.IsValidHexColor(locationCategory.Color))
            {
                return BadRequest("Invalid location category data.");
            }

            try
            {
                _context.LocationCategories.Add(locationCategory);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = locationCategory.Id,
                    wishlistId = locationCategory.WishlistId,
                    name = locationCategory.Name,
                    color = locationCategory.Color,
                    icon = locationCategory.Icon
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Log the exception details here
                // For example, you can use a logging framework like Serilog, NLog, etc.
                // Log.Error(dbEx, "A database update exception occurred while creating the location category.");

                return StatusCode(500, "A database error occurred while creating the location category.");
            }
            catch (Exception ex)
            {
                // Log the exception details here
                // Log.Error(ex, "An unexpected error occurred while creating the location category.");

                return StatusCode(500, "An unexpected error occurred while creating the location category.");
            }

        }

        //修改locationCategory OK
        //"id","wishlistId","name","color","icon"
        //PUT:api/Wishlist/UpdateLocationCategory/{id}
        [HttpPut("UpdateLocationCategory/{id}")]
        public async Task<IActionResult> UpdateLocationCategory(int id,[FromBody]LocationCategoryDTO locationCategoryDTO)
        {
            // 取得標籤
            var locationCategory = await _context.LocationCategories.FindAsync(id);
            if (locationCategory == null || locationCategory.WishlistId != locationCategoryDTO.WishlistId)
            {
                return BadRequest("無此標籤");
            }

            // 更新標籤
            locationCategory.Name = locationCategoryDTO.Name;
            locationCategory.Icon = locationCategoryDTO.Icon;
            locationCategory.Color = locationCategoryDTO.Color;

            // 保存更改
            _context.Entry(locationCategory).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Content("修改成功！");
        }
        private bool LocationCategoryExists(int id)
        {
            return _context.LocationCategories.Any(e => e.Id == id);
        }

        //刪除locationCategory OK
        //DELETE:api/Wishlist/DeleteLocationCategory/{id}
        [HttpDelete("DeleteLocationCategory/{id}")]
        public async Task<IActionResult> DeleteLocationCategory(int id)
        {
            var locationCategory = await _context.LocationCategories.FindAsync(id);
            if (locationCategory == null)
            {
                return NotFound();
            }

            // 刪除相關的wishlistDetail
            var relatedWishlistDetails = _context.WishlistDetails
                .Where(detail => detail.LocationCategoryId == id);
            _context.WishlistDetails.RemoveRange(relatedWishlistDetails);

            // 刪除locationCategory
            _context.LocationCategories.Remove(locationCategory);
            await _context.SaveChangesAsync();

            return Content("標籤及相關項目刪除成功!");
        }


        //刪除願望清單的行程 OK
        //DELETE: api/Wishlist/DeleteWishlistDetail/{id}
        [HttpDelete("DeleteWishlistDetail/{id}")]
        public async Task<IActionResult> DeleteWishlistDetail(int id)
        {
            var wishlistDetail = await _context.WishlistDetails.FindAsync(id);
            if (wishlistDetail == null)
            {
                return NotFound();
            }

            _context.WishlistDetails.Remove(wishlistDetail);
            await _context.SaveChangesAsync();

            return Content("刪除成功！");
        }
    }
}
