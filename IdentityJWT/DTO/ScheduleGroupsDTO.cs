namespace PinPinServer.DTO
{
    public class ScheduleGroupsDTO 
    {
        //WEI
        public int Id { get; set; }

        public int ScheduleId { get; set; }

        public int UserId { get; set; }

        public List<GroupMemberDTO> Members { get; set; }
    }
}
