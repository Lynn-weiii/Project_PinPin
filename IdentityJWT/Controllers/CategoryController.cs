using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;

namespace PinPinTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class categoryController : ControllerBase
    {
        private PinPinContext _context;

        public categoryController(PinPinContext context)
        {
            _context = context;
        }

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

        //GET:api/category/Getcurrency_category
        [HttpGet("GetCurrency_category")]
        public async Task<ActionResult<Dictionary<int, string>>> GetCurrency_category()
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
    }
}
