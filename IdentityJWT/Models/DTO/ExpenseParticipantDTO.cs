namespace PinPinServer.Models.DTO
{
    public class ExpenseParticipantDTO
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public float Amount { get; set; }

        public bool? IsPaid { get; set; } = false;
    }
}
