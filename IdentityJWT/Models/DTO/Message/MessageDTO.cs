using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:IdentityJWT/Models/DTO/Message/MessageDTO.cs
namespace PinPinServer.Models.DTO.Message
========
namespace PinPinServer.Models.DTO
>>>>>>>> dev:IdentityJWT/Models/DTO/SendMessageDTO.cs
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
