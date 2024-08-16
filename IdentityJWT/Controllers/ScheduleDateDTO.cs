namespace PinPinServer.Controllers
{
    public class ScheduleDateDTO
    {
        public int? Schedule_Day_Id { get; set;}

        public DateOnly? Date { get; set; }

        public Dictionary<int, DateOnly> ScheduleDates { get; set; }
    }
}