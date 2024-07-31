using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using System.Collections.Immutable;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PinPinServer.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly PinPinContext _context;

        public WishlistController(PinPinContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //取得user所有願望清單
        //GET:api/Wishlist/GetAllWishlist/{userId}
        [HttpGet("GetAllWishlist/{userId}")]
        public async Task<ActionResult<IEnumerable<WishlistDTO>>> GetAllWishlist(int userId)
        {
            var wishlists = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (wishlists == null || !wishlists.Any())
            {
                return NotFound();
            }

            var locationCategories = await _context.LocationCategories
                .Where(l => l.UserId == userId)
                .ToListAsync();

            var result = wishlists.Select(w => new WishlistDTO
            {
                Wishlists = w,
                LocationCategories = locationCategories
            }).ToList();

            return Ok(result);
        }

        //取得user所有願望清單子標籤
        //GET:api/Wishlist/GetAllLocationCategories/{userId}
        [HttpGet("GetAllLocationCategories/{userId}")]
        public async Task<ActionResult<IEnumerable<LocationCategory>>> GetAllLocationCategories(int userId) 
        {
            var locationCategories = await _context.LocationCategories
                .Where(l => l.UserId == userId)
                .ToListAsync();

            if (locationCategories == null || !locationCategories.Any()) 
            {
                return NotFound();
            }
            return Ok(locationCategories);
        }
    }
}
