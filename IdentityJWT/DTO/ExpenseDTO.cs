namespace PinPinServer.DTO
{
    public class ExpenseDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Schedule { get; set; }

        public string? Payer { get; set; }

        public string? Category { get; set; }

        public string? Currency { get; set; }

        public decimal Amount { get; set; }

        public string? Remark { get; set; }
    }
}
