using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO
{
    public class ScheduleDTO
    {
        public int Id { get; set; }

        [Display(Name = "行程名稱")]
        public string? Name { get; set; }

        [Display(Name = "出發日期")]
        public DateOnly StartTime { get; set; }

        [Display(Name = "結束日期")]
        public DateOnly EndTime { get; set; }

        public int UserId { get; set; }

        [Display(Name = "編輯者")]
        public string? UserName { get; set; }
        [Display(Name = "創建日期")]
        public DateTime? CreatedAt { get; set; }
    }
}