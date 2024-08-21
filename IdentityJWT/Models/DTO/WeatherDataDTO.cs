namespace PinPinServer.Models.DTO
{
    public class WeatherDataDTO
    {
        public double Temp { get; set; }

        public DateTime DateTime { get; set; }

        public double ChanceOfRain { get; set; }

        //true為上午fales為下午
        public bool IsMorning { get; set; }

        //濕度
        public int Humidity { get; set; }

        public double WindSpeed { get; set; }

        public string CityName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
    }
}
