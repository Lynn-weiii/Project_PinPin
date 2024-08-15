namespace PinPinServer.Models.DTO
{
    public class ChatRoomDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public bool? IsFocus { get; set; } = false;
    }
}
