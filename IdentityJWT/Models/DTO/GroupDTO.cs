namespace PinPinServer.Models.DTO
{
    public class GroupDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public List<int> AuthorityIds { get; set; }

        public bool CanRemove { get; set; }

        public int HostID { get; set; }
    }
}