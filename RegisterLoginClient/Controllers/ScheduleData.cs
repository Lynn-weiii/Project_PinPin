namespace PinPinClient.Controllers
{
    public class ScheduleData
    {
        public int ScheduleId { get; set; }
        public DateOnly StartTime { get; set; }
        public DateOnly EndTime { get; set; }
        public bool CanEdit { get; set; }
    }
}