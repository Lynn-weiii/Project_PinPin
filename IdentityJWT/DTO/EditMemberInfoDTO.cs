using System.ComponentModel.DataAnnotations;

namespace PinPinServer.DTO
{
    public class EditMemberInfoDTO
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "暱稱為必填欄位!")]
        public string Name { get; set; }

        public DateOnly? Birthday { get; set; }

        public string? Photo { get; set; }

        public int? Gender { get; set; }
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email為必填欄位!")]
        public string Email { get; set; }
    }
}
