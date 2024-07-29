namespace PinPinServer.Models.DTO
{
    public class ExpenseBalanceDTO
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public float Balance { get; set; }
        //扣掉已赴的金額
        public float IsPaidBalance { get; set; }
    }
}
