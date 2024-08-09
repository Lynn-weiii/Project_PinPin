using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http;
using PinPinServer.Models.DTO;

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

        public WishlistController(PinPinContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        //取得user所有願望清單
        //GET:api/Wishlist/GetAllWishlist/{userId}
        [HttpGet("GetAllWishlist/{userId}")]
        public async Task<ActionResult<IEnumerable<WishlistDTO>>> GetAllWishlist(int userId)
        {
            var wishlists = await _context.Wishlists
        .Include(w => w.LocationCategories)
        .Include(w => w.WishlistDetails)
        .Where(w => w.UserId == userId)
        .ToListAsync();

            if (wishlists == null || !wishlists.Any())
            {
                return NotFound();
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
                    Color = lc.Color
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
                return BadRequest("Invalid data.");
            }

            //判斷是否已存在清單
            var existingDetail = await _context.WishlistDetails
       .FirstOrDefaultAsync(wd => wd.WishlistId == wishlistDetailDTO.WishlistId && wd.GooglePlaceId == wishlistDetailDTO.GooglePlaceId);

            if (existingDetail != null)
            {
                return BadRequest("This place is already in the wishlist.");
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

            return Ok(wishlistDetail);
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
        /*"id","userId","name"*/
        //PUT:api/Wishlist/UpdateWishlist/{id}
        [HttpPut("UpdateWishlist/{id}")]
        public async Task<IActionResult> UpdateWishlist(int id, Wishlist wishlist) 
        {
            if (id != wishlist.Id)
            {
                return BadRequest();
            }

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

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();

            return Content("願望清單刪除成功!");
        }


        //新增locationCategory OK
        //POST:api/Wishlist/CreateLocationCategory
        [HttpPost("CreateLocationCategory")]
        public async Task<ActionResult<LocationCategory>> CreateLocationCategory(LocationCategory locationCategory)
        {
            if (locationCategory == null || string.IsNullOrWhiteSpace(locationCategory.Name) || string.IsNullOrWhiteSpace(locationCategory.Color))
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
        public async Task<IActionResult> UpdateLocationCategory(int id,[FromBody]LocationCategory locationCategory)
        {
            if (id != locationCategory.Id)
            {
                return BadRequest("ID mismatch.");
            }

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

            _context.LocationCategories.Remove(locationCategory);
            await _context.SaveChangesAsync();

            return Content("標籤刪除成功!");
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
