using PinPinServer.Models.DTO;
using System.Text.Json;

namespace PinPinServer.Services
{
    public class WeatherService
    {
        private readonly string _apiKey = string.Empty;
        private readonly HttpClient _httpClient;

        public WeatherService(string? apiKey, HttpClient httpClient)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            _apiKey = apiKey;
            _httpClient = httpClient;
        }

        //傳城市id，選擇測量單位
        //回傳白天+早上的
        //dto=>temp,date,rain%
        public async Task<string> GetWeatherData(string units, int cityid)
        {
            string weatherAPI = $"/data/2.5/forecast?id={cityid}appid={_apiKey}&units={units}";
            HttpResponseMessage response = await _httpClient.GetAsync(weatherAPI);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<List<WeatherDataDTO>> ProcessWeatherData(string data)
        {
            List<WeatherDataDTO> weatherDataDTOs = new List<WeatherDataDTO>();
            JsonDocument jd = JsonDocument.Parse(data);
            JsonElement root = jd.RootElement;

            foreach (JsonElement element in root.GetProperty("list").EnumerateArray())
            {
                int element.GetProperty("dt").GetInt32();
            }
        }
    }
}
