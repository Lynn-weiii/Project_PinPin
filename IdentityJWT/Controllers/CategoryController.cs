using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;
using PinPinServer.Utilities;

namespace PinPinTest.Controllers
{
    /// <summary>
    /// 這邊會處理使用者無法自訂的類別，包含SplitCategories、CurrencyCategory、FavorCategory
    /// 目前還沒有設修改權限
    /// </summary>
    [Route("api/[controller]")]
    public class categoryController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;

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
                     .Where(sc => sc.IsDeleted == false)
                     .ToDictionaryAsync(category => category.Id, category => category.Category);

                if (results.Count == 0) return NotFound("category not found");

                return Ok(results);
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        //GET:api/category/GetCurrencyCategory
        [HttpGet("GetCurrencyCategory")]
        public async Task<ActionResult<Dictionary<int, string>>> GetCurrencyCategory()
        {
            try
            {
                var results = await _context.CostCurrencyCategories
                    .Where(ccc => ccc.IsDeleted == false)
                    .ToDictionaryAsync(category => category.Id, category => category.Code);

                if (results.Count == 0) return NotFound("category not found");

                return Ok(results);
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }


        //取得喜好項目
        //GET:api/category/GetFavorCategories
        [HttpGet("GetFavorCategories")]
        public async Task<IActionResult> GetFavorCategories()
        {
            var categories = await _context.FavorCategories
                .Select(c => new { c.Id, c.Category }).ToListAsync(); // 假设Id和Name是你表中的列

            return Ok(categories);
        }






        //-------------------------------CREATE----------------------------------------

        //Post:api/category/PostSplitCategories
        /// <summary>
        /// 創建新的SplitCategories
        /// </summary>
        /// <returns>寫入成功會回傳Id,Category,Color，如果為復原刪除過的項目會多回傳Message</returns>
        [Authorize]
        [HttpPost("PostSplitCategories")]
        public async Task<ActionResult> PostSplitCategories([FromForm] string category, [FromForm] string color)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(color))
                return BadRequest("Category or color is required.");

            //檢查色碼格式
            if (!Validator.IsValidHexColor(color)) return BadRequest("Color format error");

            //檢查有無重複值，如果是刪除過的將它復原
            SplitCategory? DuplicatedSC = await _context.SplitCategories.FirstOrDefaultAsync(sc => sc.Category == category);
            if (DuplicatedSC != null)
            {
                if (DuplicatedSC.IsDeleted == false) return BadRequest("Category is Duplicated");

                try
                {
                    DuplicatedSC.IsDeleted = false;
                    DuplicatedSC.Color = color;

                    _context.Update(DuplicatedSC);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        DuplicatedSC.Id,
                        DuplicatedSC.Category,
                        DuplicatedSC.Color,
                        Message = "Category reactivated and updated"
                    });
                }
                catch { return StatusCode(500, "A Database error."); }
            }

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
                return StatusCode(500, "A Database error.");
            }
        }

        //Post:api/category/PostCurrencyCategory
        /// <summary>
        /// 創建新的CurrencyCategory
        /// </summary>
        /// <returns>寫入成功會回傳Id,Code,Name，如果為復原刪除過的項目會多回傳Message</returns>
        [Authorize]
        [HttpPost("PostCurrencyCategory")]
        public async Task<ActionResult> PostCurrencyCategory([FromForm] string code, [FromForm] string name)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
                return BadRequest("code or name is required.");

            //檢查幣別格式
            if (!Validator.IsValidCurrency(code)) return BadRequest("Code format error");

            //檢查有無重複值，如果是刪除過的將它復原
            CostCurrencyCategory? DuplicatedCC = await _context.CostCurrencyCategories.FirstOrDefaultAsync(cc => cc.Code == code);
            if (DuplicatedCC != null)
            {
                if (DuplicatedCC.IsDeleted == false) return BadRequest("Category is Duplicated");

                DuplicatedCC.IsDeleted = false;
                DuplicatedCC.Name = name;

                try
                {
                    _context.Update(DuplicatedCC);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        DuplicatedCC.Id,
                        DuplicatedCC.Code,
                        DuplicatedCC.Name,
                        Message = "Category reactivated and updated"
                    });
                }
                catch { return StatusCode(500, "A Database error."); }
            }

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
                return StatusCode(500, "A Database error.");
            }
        }

        //Post:api/category/PostFavorCategory
        /// <summary>
        /// 創建新的FavorCategory
        /// </summary>
        /// <returns>寫入成功會回傳Id,Category，如果為復原刪除過的項目會多回傳Message</returns>
        [Authorize]
        [HttpPost("PostFavorCategory")]
        public async Task<ActionResult> PostFavorCategory([FromForm] string category)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category is required.");

            //檢查有無重複值，如果是刪除過的將它復原
            FavorCategory? DuplicatedFC = await _context.FavorCategories.FirstOrDefaultAsync(fc => fc.Category == category);
            if (DuplicatedFC != null)
            {
                if (DuplicatedFC.IsDeleted == false) return BadRequest("Category is Duplicated");

                DuplicatedFC.IsDeleted = false;
                DuplicatedFC.Category = category;

                try
                {
                    _context.Update(DuplicatedFC);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        DuplicatedFC.Id,
                        DuplicatedFC.Category,
                        Message = "Category reactivated and updated"
                    });
                }
                catch { return StatusCode(500, "A Database error."); }
            }

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
                return StatusCode(500, "A Database error.");
            }
        }

        //-------------------------------UPDATE----------------------------------------

        //Put:api/category/PutSplitCategories
        /// <summary>
        /// 修改SplitCategories，依據dto中的ID去尋找需要修改的值
        /// </summary>
        /// <returns>修改成功會回傳Category,Color</returns>
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

            //檢查有無重複值或已經刪除
            bool isDuplicated = await _context.SplitCategories.AnyAsync(sc => sc.Category == dto.Category);
            if (isDuplicated) return BadRequest("Category is Duplicated or Delete");

            SplitCategory? splitCategory = await _context.SplitCategories.FirstOrDefaultAsync(sc => sc.Id == dto.Id);
            if (splitCategory == null || splitCategory.Id == 0) return BadRequest("Not found Category");

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
                return StatusCode(500, "A Database error.");
            }
        }

        //Put:api/category/PutCurrencyCategory
        /// <summary>
        /// 修改CurrencyCategory，依據dto中的ID去尋找需要修改的值
        /// </summary>
        /// <returns>修改成功會回傳Code,Name</returns>
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
            if (isDuplicated) return BadRequest("Code is Duplicated or Delete");

            CostCurrencyCategory? costCurrency = await _context.CostCurrencyCategories.FirstOrDefaultAsync(ccc => ccc.Id == dto.Id);
            if (costCurrency == null || costCurrency.Id == 0) return BadRequest("Not found Category");

            try
            {
                costCurrency.Code = dto.Code;
                costCurrency.Name = dto.Name;
                costCurrency.Icon = dto.Icon;

                _context.Update(costCurrency);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    costCurrency.Code,
                    costCurrency.Name,
                    costCurrency.Icon,
                });
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }

        //Put:api/category/PutFavorCategory
        /// <summary>
        /// 修改PutFavorCategory，依據dto中的ID去尋找需要修改的值
        /// </summary>
        /// <returns>修改成功會回傳Category</returns>
        [Authorize]
        [HttpPut("PutFavorCategory")]
        public async Task<ActionResult> PutFavorCategory([FromForm] int id, [FromForm] string category)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            FavorCategory? favorCategory = await _context.FavorCategories.FirstOrDefaultAsync(fc => fc.Id == id);
            if (favorCategory == null || favorCategory.Id == 0) return BadRequest("Not found Category");

            //檢查輸入值是否為空
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category is required.");

            //檢查有無重複值
            bool isDuplicated = await _context.FavorCategories.AnyAsync(fc => fc.Category == category);
            if (isDuplicated) return BadRequest("Code is Duplicated or Delete");

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
                return StatusCode(500, "A Database error.");
            }
        }

        //-------------------------------DELETED----------------------------------------

        //Delete:api/category/DeleteSplitCategories/{id}
        [Authorize]
        [HttpDelete("DeleteSplitCategories")]
        public async Task<ActionResult<string>> DeleteSplitCategories(int id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            SplitCategory? splitCategory = await _context.SplitCategories.FirstOrDefaultAsync(sc => sc.Id == id);
            if (splitCategory == null || splitCategory.Id == 0) return BadRequest("Not found Category");
            try
            {
                splitCategory.IsDeleted = true;

                _context.Update(splitCategory);
                await _context.SaveChangesAsync();

                return Ok($"Category {splitCategory.Category} successfully deleted");
            }
            catch
            {
                return StatusCode(500, new { Error = "A Database error." });
            }
        }

        //Delete:api/category/DeleteCurrencyCategory/{id}
        [Authorize]
        [HttpDelete("DeleteCurrencyCategory")]
        public async Task<ActionResult<string>> DeleteCurrencyCategory(int id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            CostCurrencyCategory? costCurrencyCategory = await _context.CostCurrencyCategories.FirstOrDefaultAsync(ccc => ccc.Id == id);
            if (costCurrencyCategory == null || costCurrencyCategory.Id == 0) return BadRequest("Not found Category");
            try
            {
                costCurrencyCategory.IsDeleted = true;

                _context.Update(costCurrencyCategory);
                await _context.SaveChangesAsync();

                return Ok($"Category {costCurrencyCategory.Name} successfully deleted");
            }
            catch
            {
                return StatusCode(500, new { Error = "A Database error." });
            }
        }

        //Delete:api/category/DeleteFavorCategory/{id}
        [Authorize]
        [HttpDelete("DeleteFavorCategory")]
        public async Task<ActionResult<string>> DeleteFavorCategory(int id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            FavorCategory? favorCategory = await _context.FavorCategories.FirstOrDefaultAsync(ccc => ccc.Id == id);
            if (favorCategory == null || favorCategory.Id == 0) return BadRequest("Not found Category");
            try
            {
                favorCategory.IsDeleted = true;

                _context.Update(favorCategory);
                await _context.SaveChangesAsync();

                return Ok($"Category {favorCategory.Category} successfully deleted");
            }
            catch
            {
                return StatusCode(500, new { Error = "A Database error." });
            }
        }
    }
}
