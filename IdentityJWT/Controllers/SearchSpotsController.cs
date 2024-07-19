using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchSpotsController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SearchSpotsController> _logger;

        public SearchSpotsController(PinPinContext context,IHttpClientFactory httpClientFactory, ILogger<SearchSpotsController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get([FromQuery] string q)
        //{
        //    if (string.IsNullOrEmpty(q))
        //    {
        //        return BadRequest(new { error = "Query parameter is required" });
        //    }

        //    try
        //    {
        //        var url = $"https://nominatim.openstreetmap.org/search?q={System.Net.WebUtility.UrlEncode(q)}&format=json&addressdetails=1";
        //        var response = await _httpClient.GetStringAsync(url);
        //        return Ok(response);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError("Error fetching data from Nominatim API: {0}", ex);
        //        return StatusCode(500, new { error = "Failed to fetch data" });
        //    }
        //}


        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://nominatim.openstreetmap.org/search?q={query}&format=json&addressdetails=1";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

      


        //老師課堂上的篩選關鍵字功能，pinpin只會有篩選景點名稱、地點描述(open street做得到嗎?)
        //POST:api/Categories/Filter
        //[HttpPost("Filter")]
        //public async Task<IEnumerable<Category>> FilterCategory([FromBody] Category category)
        //{

        //    return _context.Categories.Where(c =>

        //    c.CategoryId == category.CategoryId ||
        //    c.CategoryName.Contains(category.CategoryName) ||
        //    c.Description.Contains(category.Description)).
        //    Select(c => new Category
        //    {
        //        CategoryId = c.CategoryId,
        //        CategoryName = c.CategoryName,
        //        Description = c.Description,
        //        Picture = null
        //    });
        //}
    }
}
