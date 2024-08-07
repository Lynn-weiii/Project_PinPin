namespace PinPinServer.Models.DTO
{
    public class ScheduleMemberDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SchdeleId { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime? LeftDate { get; set; }

    }

}
