using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO.Message
{
    public class MessageDTO
    {
        [Required]
        public int ScheduleId { get; set; }

        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
