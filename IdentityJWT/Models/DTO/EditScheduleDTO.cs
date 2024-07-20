using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO
{
    public class EditScheduleDTO
    {

        [Display(Name = "行程名稱")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "出發日期")]
        public DateOnly StartTime { get; set; }

        [Display(Name = "結束日期")]
        public DateOnly EndTime { get; set; }

        public int UserId { get; set; }



        //允許空值

        public int? Id { get; set; }

        [Display(Name = "創建日期")]
        public DateTime? CreatedAt { get; set; }
    }
}