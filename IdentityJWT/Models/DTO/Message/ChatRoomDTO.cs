namespace PinPinServer.Models.DTO.Message
{
    public class ChatRoomDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public bool? IsFocus { get; set; } = false;

        public int ScheduleId { get; set; }

        public bool? IsEdited { get; set; } = false;

        public bool? IsDeleted { get; set; }

        public DateTime? LastEditedAt { get; set; } 
    }
}
