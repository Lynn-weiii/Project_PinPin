using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}