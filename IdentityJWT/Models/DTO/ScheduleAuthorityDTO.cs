namespace PinPinServer.Models.DTO
{
    internal class ScheduleAuthorityDTO
    {

        public string UserName { get; set; }
        public int ScheduleId { get; set; }
        public List<int> AuthorityCategoryIds { get; set; } // 修正属性类型为 List<int>
        public int UserId { get; set; }
    }
}