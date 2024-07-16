using System.ComponentModel.DataAnnotations;

namespace PinPinServer.DTO
{
    public class EditScheduleDTO
    {

        [Display(Name = "行程名稱")]
        [Required(ErrorMessage = "請輸入行程名稱!")]
        public string Name { get; set; }

        [Display(Name = "出發日期")]
        [Required(ErrorMessage = "請選擇出發日期!")]
        public DateOnly StartTime { get; set; } // 改为字符串以便处理

        [Display(Name = "結束日期")]
        [Required(ErrorMessage = "請選擇結束日期!")]
        public DateOnly EndTime { get; set; } // 改为字符串以便处理

        public int UserId { get; set; }

        //允許空值
        public int? Id { get; set; }

        [Display(Name = "創建日期")]
        public DateTime? CreatedAt { get; set; }
    }
}
