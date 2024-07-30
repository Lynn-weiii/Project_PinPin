namespace PinPinServer.Models.DTO.Expense
{
    /// <summary>
    /// 紀錄使用者與個別團員的資料
    /// </summary>
    public class ExpenseUserDTO
    {
        public int ExpenseId { get; set; }

        public string? ExpenseName { get; set; }

        public string? ExpenseCategory { get; set; }

        public float Amount { get; set; }

        public string? CostCategory { get; set; }

        public bool? IsPaid { get; set; }
    }
}
