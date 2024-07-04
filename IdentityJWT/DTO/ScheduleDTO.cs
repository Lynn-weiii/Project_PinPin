namespace PinPinServer.DTO
{
    public class ScheduleDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public DateOnly StartTime { get; set; }

        public DateOnly EndTime { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int UserId { get; set; }
    }
}