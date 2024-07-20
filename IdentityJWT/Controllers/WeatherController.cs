using Microsoft.AspNetCore.Mvc;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{

    //回傳白天+早上的
    //dto=>temp,date,rain%
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly AuthGetuserId _getUserId;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string API_key = "";
        private readonly string units = "metric";
        private readonly string lang = "zh_tw";

        public WeatherController(AuthGetuserId getuserId)
        {
            _getUserId = getuserId;
        }

        //傳城市id，選擇測量單位
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
