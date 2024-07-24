namespace PinPinServer.Models.DTO
{
    public class WeatherDataDTO
    {
        public double Temp { get; set; }

        public DateTime DateTime { get; set; }

        public double ChanceOfRain { get; set; }

        //true為上午fales為下午
        public bool IsMorning { get; set; }
    }
}
