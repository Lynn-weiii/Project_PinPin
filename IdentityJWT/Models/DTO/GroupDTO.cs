namespace PinPinServer.Models.DTO
{
    public class GroupDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public List<int> AuthorityIds { get; set; }
        public bool Removeauth { get; set; }
        public bool Canremoveid { get; set; }
        public bool IsHost { get; set; }
    }
}