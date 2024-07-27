using PinPinServer.Models.DTO;
using System.Text.Json;

namespace PinPinServer.Services
{
    public class WeatherService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public WeatherService(string? apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        //呼叫獲取天氣資料的API
        public async Task<string> GetWeatherData(string units, double lat, double lon)
        {
            string baseaddress = $"BaseAddress: {_httpClient.BaseAddress}";
            string weatherAPI = $"data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units={units}";
            HttpResponseMessage response = await _httpClient.GetAsync(weatherAPI);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("獲取資料失敗");
                return string.Empty;
            }
            return ProcessWeatherData(await response.Content.ReadAsStringAsync());
        }

        //處理獲取的資料
        private string ProcessWeatherData(string data)
        {
            List<WeatherDataDTO> weatherDataList = new List<WeatherDataDTO>();
            JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;

            //提取每個時間段的資料
            foreach (JsonElement jsonElement in root.GetProperty("list").EnumerateArray())
            {
                int unixTime = jsonElement.GetProperty("dt").GetInt32();
                double chanceOfRain = jsonElement.GetProperty("pop").GetDouble();
                double temp = jsonElement.GetProperty("main").GetProperty("temp").GetDouble();

                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;

                weatherDataList.Add(new WeatherDataDTO
                {
                    DateTime = date,
                    ChanceOfRain = chanceOfRain,
                    Temp = temp,
                    IsMorning = date.Hour < 12
                });
            }

            //計算每天上下午的平均降雨機率和氣溫
            var groupedData = weatherDataList
                .GroupBy(data => new { data.DateTime.Date, data.IsMorning })
                .Select(g => new WeatherDataDTO
                {
                    DateTime = g.Key.Date,
                    IsMorning = g.Key.IsMorning,
                    Temp = g.Average(data => data.Temp),
                    ChanceOfRain = g.Average(data => data.ChanceOfRain)
                })
                .GroupBy(data => data.DateTime)
                .Select(day => new
                {
                    Day = day.ToList()
                })
                .ToList();

            //轉成JSON格式
            string jsonString = JsonSerializer.Serialize(groupedData);
            return jsonString;
        }
    }
}
