using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.DTO;
using PinPinServer.Models;
using PinPinServer.Services;
using PinPinServer.Utilities;

namespace PinPinTest.Controllers
{
    /// <summary>
    /// 這邊會處理使用者無法自訂的類別，包含SplitCategories、CurrencyCategory、FavorCategory
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class categoryController : ControllerBase
    {
        private PinPinContext _context;
        private AuthGetuserId _getUserId;

        public categoryController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        //------------------------------GET-------------------------------------

        //GET:api/category/GetSplitCategories
        [HttpGet("GetSplitCategories")]
        public async Task<ActionResult<Dictionary<int, string>>> GetSplitCategories()
        {
            try
            {
                var results = await _context.SplitCategories
                     .Select(category => category)
                     .ToDictionaryAsync(category => category.Id, category => category.Category);

                if (results.Count == 0) return NotFound("category not found");

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //GET:api/category/GetCurrencyCategory
        [HttpGet("GetCurrencyCategory")]
        public async Task<ActionResult<Dictionary<int, string>>> GetCurrencyCategory()
        {
            try
            {
                var results = await _context.CostCurrencyCategories
                    .Select(category => category)
                    .ToDictionaryAsync(category => category.Id, category => category.Code);

                if (results.Count == 0) return NotFound("category not found");

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //GET:api/category/GetFavorCategory
        [HttpGet("GetFavorCategory")]
        public async Task<ActionResult<Dictionary<int, string>>> GetFavorCategory()
        {
            try
            {
                var results = await _context.FavorCategories
                    .Select(category => category)
                    .ToDictionaryAsync(category => category.Id, category => category.Category);

                if (results.Count == 0) return NotFound("category not found");

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //-------------------------------CREATE----------------------------------------

        //Post:api/category/PostSplitCategories
        [Authorize]
        [HttpPost("PostSplitCategories")]
        public async Task<ActionResult> PostSplitCategories([FromForm] string category, [FromForm] string color)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(color))
                return BadRequest("Category or color is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.SplitCategories.AnyAsync(sc => sc.Category == category);
            if (isDuplicated) return BadRequest("Category is Duplicated");

            //檢查色碼格式
            if (!Validator.IsValidHexColor(color)) return BadRequest("Color format error");

            SplitCategory splitCategory = new SplitCategory
            {
                Category = category,
                Color = color,
            };

            try
            {
                _context.SplitCategories.Add(splitCategory);
                await _context.SaveChangesAsync();

                var result = await _context.SplitCategories.Where(sc => sc.Id == splitCategory.Id).Select(sc => new
                {
                    sc.Id,
                    sc.Category,
                    sc.Color
                }).FirstOrDefaultAsync();

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }

        //Post:api/category/PostCurrencyCategory
        [Authorize]
        [HttpPost("PostCurrencyCategory")]
        public async Task<ActionResult> PostCurrencyCategory([FromForm] string code, [FromForm] string name)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
                return BadRequest("code or name is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.CostCurrencyCategories.AnyAsync(ccc => ccc.Code == code);
            if (isDuplicated) return BadRequest("Code is Duplicated");

            //檢查幣別格式
            if (!Validator.IsValidCurrency(code)) return BadRequest("Code format error");

            CostCurrencyCategory costCurrencyCategory = new CostCurrencyCategory
            {
                Code = code,
                Name = name,
            };

            try
            {
                _context.CostCurrencyCategories.Add(costCurrencyCategory);
                await _context.SaveChangesAsync();

                var result = await _context.CostCurrencyCategories.Where(ccc => ccc.Id == costCurrencyCategory.Id).Select(ccc => new
                {
                    ccc.Id,
                    ccc.Code,
                    ccc.Name
                }).FirstOrDefaultAsync();

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }

        //Post:api/category/PostFavorCategory
        [Authorize]
        [HttpPost("PostFavorCategory")]
        public async Task<ActionResult> PostFavorCategory([FromForm] string category)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.FavorCategories.AnyAsync(fc => fc.Category == category);
            if (isDuplicated) return BadRequest("Code is Duplicated");

            FavorCategory favorCategory = new FavorCategory
            {
                Category = category,
            };

            try
            {
                _context.FavorCategories.Add(favorCategory);
                await _context.SaveChangesAsync();

                var result = await _context.FavorCategories.Where(fc => fc.Id == favorCategory.Id).Select(fc => new
                {
                    fc.Id,
                    fc.Category,
                }).FirstOrDefaultAsync();

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }

        //-------------------------------UPDATE----------------------------------------

        //Put:api/category/PutSplitCategories
        [Authorize]
        [HttpPut("PutSplitCategories")]
        public async Task<ActionResult> PutSplitCategories([FromBody] SplitCategoriesDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage).ToList());
                return BadRequest(new { Error = errors });
            }

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(dto.Category))
                return BadRequest("Category is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.SplitCategories.AnyAsync(sc => sc.Category == dto.Category);
            if (isDuplicated) return BadRequest("Category is Duplicated");

            SplitCategory? splitCategory = await _context.SplitCategories.FirstOrDefaultAsync(sc => sc.Id == dto.Id);
            if (splitCategory == null || splitCategory.Id == 0) return BadRequest("Not found SplitCategory");

            try
            {
                splitCategory.Category = dto.Category;
                splitCategory.Color = dto.Color;

                _context.Update(splitCategory);
                await _context.SaveChangesAsync();



                return Ok(new
                {
                    splitCategory.Category,
                    splitCategory.Color,
                });
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }

        //Put:api/category/PutCurrencyCategory
        [Authorize]
        [HttpPut("PutCurrencyCategory")]
        public async Task<ActionResult> PutCurrencyCategory([FromBody] CostCategoryDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage).ToList());
                return BadRequest(new { Error = errors });
            }

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(dto.Name))
                return BadRequest("Name is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.CostCurrencyCategories.AnyAsync(ccc => ccc.Code == dto.Code);
            if (isDuplicated) return BadRequest("Code is Duplicated");

            CostCurrencyCategory? costCurrency = await _context.CostCurrencyCategories.FirstOrDefaultAsync(ccc => ccc.Id == dto.Id);
            if (costCurrency == null || costCurrency.Id == 0) return BadRequest("Not found SplitCategory");

            try
            {
                costCurrency.Code = dto.Code;
                costCurrency.Name = dto.Name;

                _context.Update(costCurrency);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    costCurrency.Code,
                    costCurrency.Name,
                });
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }

        //Put:api/category/PutFavorCategory
        [Authorize]
        [HttpPut("PutFavorCategory")]
        public async Task<ActionResult> PutFavorCategory([FromForm] int id, [FromForm] string category)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            FavorCategory? favorCategory = await _context.FavorCategories.FirstOrDefaultAsync(fc => fc.Id == id);
            if (favorCategory == null || favorCategory.Id == 0) return BadRequest("Not found SplitCategory");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.FavorCategories.AnyAsync(fc => fc.Category == category);
            if (isDuplicated) return BadRequest("Code is Duplicated");

            try
            {
                favorCategory.Category = category;

                _context.Update(favorCategory);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    favorCategory.Category,
                });
            }
            catch
            {
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense." });
            }
        }
    }
}
