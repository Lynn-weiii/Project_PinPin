using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationCategoryController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;
        //未選擇的項目不包括在裡面
        private readonly Lazy<List<LocationCategory>> _defaultCategories;
        private readonly LocationCategory _unselectedCategory;
        private readonly static string _unselectStr = "未選擇";

        public LocationCategoryController(PinPinContext context, AuthGetuserId getUserId)
        {
            _context = context;
            _getUserId = getUserId;
            _defaultCategories = new Lazy<List<LocationCategory>>(() => GetDefaultCategory(1));
            _unselectedCategory = GetUnselectedCategory(_defaultCategories.Value, _unselectStr);
        }

        private List<LocationCategory> GetDefaultCategory(int userId)
        {
            List<LocationCategory> categories = _context.LocationCategories.AsNoTracking().Where(lc => lc.UserId == userId).ToList();
            if (categories.Count == 0)
            {
                throw new Exception("Default category not found");
            }
            return categories;
        }

        private LocationCategory GetUnselectedCategory(List<LocationCategory> categories, string name)
        {
            LocationCategory? locationCategory = categories.FirstOrDefault(lc => lc.Name == name);
            if (locationCategory == null)
            {
                throw new Exception("Unselect category not found");
            }
            categories.Remove(locationCategory);
            return locationCategory;
        }

        //GET:api/LocationCategoryController
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationCategoryDTO>>> Get()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            try
            {
                List<LocationCategoryDTO> categories =
                    await _context.LocationCategories
                           .Where(lc => lc.UserId == userID)
                           .Select(lc => new LocationCategoryDTO
                           {
                               Id = lc.Id,
                               Color = lc.Color,
                               Name = lc.Name,
                           }).ToListAsync();

                categories.AddRange(_defaultCategories.Value.Select(dc => new LocationCategoryDTO
                {
                    Id = dc.Id,
                    Color = dc.Color,
                    Name = dc.Name,
                }));

                return Ok(categories);
            }
            catch
            { return StatusCode(500, "A Database error."); }
        }

        //GET:api/LocationCategoryController/Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("Admin")]
        public ActionResult<IEnumerable<LocationCategoryDTO>> GetAdmin()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            List<LocationCategory> locationCategories = [.. _defaultCategories.Value];
            locationCategories.Add(_unselectedCategory);

            return Ok(locationCategories);
        }

        //POST:api/LocationCategoryController/Post
        /// <summary>
        /// dto中的id可以不用船
        /// </summary>
        [HttpPost("Post")]
        public async Task<ActionResult<LocationCategoryDTO>> Post([FromBody] LocationCategoryDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage).ToList());
                return BadRequest(new { Error = errors });
            }

            //檢查輸入直是否為空
            if (String.IsNullOrEmpty(dto.Name)) return BadRequest("Name is required.");

            //檢查有無重複值或顏色
            List<LocationCategory> categories = await _context.LocationCategories.Where(lc => lc.UserId == userID).AsNoTracking().ToListAsync();
            if (categories.Any(lc => lc.Name == dto.Name || lc.Color == dto.Color)) return BadRequest("Category or color is duplicated");

            LocationCategory locationCategory = new LocationCategory { Name = dto.Name, Color = dto.Color, UserId = (int)userID };
            try
            {
                _context.LocationCategories.Add(locationCategory);
                await _context.SaveChangesAsync();

                return Ok(new LocationCategoryDTO
                {
                    Id = locationCategory.Id,
                    Name = locationCategory.Name,
                    Color = locationCategory.Color,
                });
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        //PUT:api/LocationCategoryController/Put
        [HttpPut("Put")]
        public async Task<ActionResult<LocationCategoryDTO>> Put([FromBody] LocationCategoryDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage).ToList());
                return BadRequest(new { Error = errors });
            }

            //檢查輸入直是否為空
            if (String.IsNullOrEmpty(dto.Name)) return BadRequest("Category is required.");

            //檢查是否有此筆資料
            LocationCategory? locationCategory = await _context.LocationCategories.FirstOrDefaultAsync(lc => lc.Id == dto.Id);
            if (locationCategory == null) return BadRequest("Not found Category");

            //檢查是否有權限更改
            if (locationCategory.UserId != userID) return BadRequest("Don't have permission to change");

            //檢查值是否與原來不同
            if (locationCategory.Name == dto.Name && locationCategory.Color == dto.Color) return BadRequest("Name and color has not changed");

            try
            {
                locationCategory.Name = dto.Name;
                locationCategory.Color = dto.Color;

                _context.LocationCategories.Update(locationCategory);
                await _context.SaveChangesAsync();

                return Ok(new LocationCategoryDTO
                {
                    Id = locationCategory.Id,
                    Name = locationCategory.Name,
                    Color = locationCategory.Color,
                });
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        ////Delete:api/LocationCategoryController/Delete
        //[HttpDelete("Delete")]
        //public async Task<ActionResult<int>> Delete(int id)
        //{

        //}
    }
}
