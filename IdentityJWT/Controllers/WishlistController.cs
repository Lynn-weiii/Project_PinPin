using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http;

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

    }
}
