using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{

    //回傳白天+早上的
    //dto=>temp,date,rain%
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly AuthGetuserId _getUserId;
        private readonly WeatherService _weatherService;

        public WeatherController(AuthGetuserId getuserId, WeatherService weatherService)
        {
            _getUserId = getuserId;
            _weatherService = weatherService;
        }

        /// <summary>
        /// 傳入經度與緯度即可獲取資料型態為JSON的回應，單位格式為imperial和metric
        /// </summary>
        /// <returns>回應格式為JSON，架構為一個Day的列表裡頭包含兩個天氣資訊，分別為上下午，資訊為WeatherDataDTO格式</returns>
        //GET:api/Weather
        [HttpGet]
        public async Task<ActionResult> Get(decimal lat, decimal lon, string units)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //呼叫獲取函式
            var json = await _weatherService.GetWeatherData(units, lat, lon);
            if (String.IsNullOrEmpty(json)) return StatusCode(500, "Unable to get data.");

            return Ok(json);
        }
    }
}
