namespace PinPinServer.Models.DTO
{
    public class ScheduleAuthorityDTO
    {

        public string UserName { get; set; }
        public int ScheduleId { get; set; }
        public List<int> AuthorityCategoryIds { get; set; }
        public int UserId { get; set; }
        public int? Id { get; set; }
    }
}