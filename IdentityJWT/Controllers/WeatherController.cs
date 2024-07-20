using Microsoft.AspNetCore.Mvc;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly AuthGetuserId _getUserId;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string API_key = "c07f09ae7a595c84cdcc594120edf88e";
        private readonly string units = "metric";
        private readonly string lang = "zh_tw";

        public WeatherController(AuthGetuserId getuserId)
        {
            _getUserId = getuserId;
        }

        [HttpGet]
        public async Task<ActionResult> Get(decimal lat, decimal lon)
        {
            string weatherAPI = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={API_key}&units={units}&cnt=1";
            HttpResponseMessage response = await _httpClient.GetAsync(weatherAPI);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return Ok(data);
            }
            return BadRequest();
        }
    }
}
