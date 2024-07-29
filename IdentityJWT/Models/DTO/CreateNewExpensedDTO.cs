using System.ComponentModel.DataAnnotations;
using PinPinServer.Models.DTO.Expense;

namespace PinPinServer.Models.DTO
{
    public class CreateNewExpensedDTO
    {
        [Required(ErrorMessage = "Schedule ID is required.")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "Payer ID is required.")]
        public int PayerId { get; set; }

        [Required(ErrorMessage = "Split Category ID is required.")]
        public int SplitCategoryId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency ID is required.")]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public float Amount { get; set; }

        public string Remark { get; set; } = string.Empty;

        [Required(ErrorMessage = "participants is required.")]
        [MinCountAttributes(1)]
        public List<ExpenseParticipantDTO> Participants { get; set; } = [];
    }
}
