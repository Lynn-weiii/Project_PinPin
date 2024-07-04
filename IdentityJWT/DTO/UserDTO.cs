using System.ComponentModel.DataAnnotations;

namespace PinPinServer.DTO
{
    public class UserDTO
    {

        public int? Id { get; set; }

        [Required(ErrorMessage = "暱稱為必填欄位!")]
        public string Name { get; set; }

        public DateOnly? Birthday { get; set; }

        public string? Photo { get; set; } 

        public int? Gender { get; set; }

        [Required(ErrorMessage = "Email為必填欄位!")]
        public string Email { get; set; }

        public string? Phone { get; set; }

        [Required(ErrorMessage = "密碼為必填欄位")]
        public string Password { get; set; }

        [Required(ErrorMessage = "請再次確認密碼!")]
        public string PasswordConfirm { get; set; }
    }
}
