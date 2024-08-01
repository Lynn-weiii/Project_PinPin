using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using System.Text.Json;

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
        .Include(w => w.LocationCategories)
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
                }).ToList()
            }).ToList();

            return Ok(result);

        }


    }
}
