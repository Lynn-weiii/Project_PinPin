using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
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
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public SearchSpotsController(PinPinContext context,IHttpClientFactory httpClientFactory, IConfiguration configuration,IMemoryCache cache)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
        }


        //GET:api/SearchSpots/search
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];
            var client = _httpClientFactory.CreateClient();
            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={query}&key={apiKey}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        //GET:api/SearchSpots/GetPhoto
        [HttpGet("GetPhoto")]
        public IActionResult GetPhotoUrl(string photoReference)
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];
            var photoUrl = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference={photoReference}&key={apiKey}";

            return Ok(new { url = photoUrl });
        }
    }
}
