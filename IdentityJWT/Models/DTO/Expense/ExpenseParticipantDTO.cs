namespace PinPinServer.Models.DTO.Expense
{
    public class ExpenseParticipantDTO
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool? IsPaid { get; set; } = false;
    }
}
