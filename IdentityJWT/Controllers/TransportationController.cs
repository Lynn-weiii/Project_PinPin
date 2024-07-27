using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransportationController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;
        private readonly Lazy<List<TransportationCategory>> _defaultCategories;
        private readonly TransportationCategory _unselectedCategory;
        private readonly static string _unselectStr = "未選擇";

        public TransportationController(PinPinContext context, AuthGetuserId getUserId)
        {
            _context = context;
            _getUserId = getUserId;
            _defaultCategories = new Lazy<List<TransportationCategory>>(() => GetDefaultCategory(1));
            _unselectedCategory = GetUnselectedCategory(_defaultCategories.Value, _unselectStr);
        }

        private List<TransportationCategory> GetDefaultCategory(int userId)
        {
            List<TransportationCategory> categories = _context.TransportationCategories.AsNoTracking().Where(tc => tc.UserId == userId).ToList();
            if (categories.Count == 0)
            {
                throw new Exception("Default category not found");
            }
            return categories;
        }

        private TransportationCategory GetUnselectedCategory(List<TransportationCategory> categories, string name)
        {
            TransportationCategory? category = categories.FirstOrDefault(tc => tc.Name == name);
            if (category == null)
            {
                throw new Exception("Unselect category not found");
            }
            categories.Remove(category);
            return category;
        }

        //GET:api/Transportation
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransportationCategory>>> Get()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            try
            {
                List<TransportationCategoryDTO> categories =
                    await _context.TransportationCategories
                           .Where(tc => tc.UserId == userID)
                           .Select(tc => new TransportationCategoryDTO
                           {
                               Id = tc.Id,
                               Name = tc.Name,
                               IconUrl = tc.IconUrl,
                           }).ToListAsync();

                categories.AddRange(_defaultCategories.Value.Select(dc => new TransportationCategoryDTO
                {
                    Id = dc.Id,
                    Name = dc.Name,
                }));

                return Ok(categories);
            }
            catch
            { return StatusCode(500, "A Database error."); }
        }

        //GET:api/Transportation/Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("Admin")]
        public ActionResult<IEnumerable<TransportationCategoryDTO>> GetAdmin()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            if (_defaultCategories.Value == null || _unselectedCategory == null)
            {
                return StatusCode(500, "Default categories or unselected category not found.");
            }

            List<TransportationCategory> categories = [.. _defaultCategories.Value];
            categories.Add(_unselectedCategory);

            List<TransportationCategoryDTO> categoryDTOs = categories.Select(c => new TransportationCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                IconUrl = c.IconUrl,
            }).ToList();

            return Ok(categoryDTOs);
        }

        //POST:api/Transportation/Post
        /// <summary>
        /// dto中的id可以不用傳
        /// </summary>
        [HttpPost("Post")]
        public async Task<ActionResult<TransportationCategoryDTO>> Post([FromBody] TransportationCategoryDTO dto)
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
            List<TransportationCategory> categories = await _context.TransportationCategories.Where(tr => tr.UserId == userID).AsNoTracking().ToListAsync();
            if (categories.Any(tr => tr.Name == dto.Name || tr.IconUrl == tr.IconUrl)) return BadRequest("Category or color is duplicated");

            TransportationCategory category = new TransportationCategory { Name = dto.Name, IconUrl = dto.IconUrl, UserId = (int)userID };
            try
            {
                _context.TransportationCategories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(new TransportationCategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    IconUrl = category.IconUrl,
                });
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }
    }
}
