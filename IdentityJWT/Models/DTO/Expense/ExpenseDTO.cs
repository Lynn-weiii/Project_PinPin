namespace PinPinServer.Models.DTO.Expense
{
    public class ExpenseDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Schedule { get; set; }

        public int PayerId { get; set; }

        public string? Payer { get; set; }

        public string? Category { get; set; }

        //Code
        public string? Currency { get; set; }

        public decimal Amount { get; set; }

        public string? Remark { get; set; }

        public string? CreatedAt { get; set; }

        public List<ExpenseParticipantDTO> ExpenseParticipants { get; set; } = [];
    }
}
