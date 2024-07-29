using PinPinServer.Models;
using PinPinServer.Models.DTO.Expense;

namespace PinPinServer.Services
{
    public class ExpenseCalculator
    {
        public List<ExpenseBalanceDTO> CalculateBalance(List<SplitExpense> expenses, List<ExpenseBalanceDTO> members, int userId)
        {
            List<SplitExpenseParticipant> payYourself = expenses.Where(e => e.PayerId == userId)
                                                                .SelectMany(e => e.SplitExpenseParticipants).ToList();
            List<SplitExpense> others = expenses.Where(e => e.PayerId != userId).ToList();

            //先算出誰欠我
            foreach (var sep in payYourself)
            {
                var member = members.First(m => m.UserId == sep.UserId);
                member.Balance += sep.Amount;
                if (sep.IsPaid == false)
                    member.IsPaidBalance += sep.Amount;
            }

            //再算我欠別人的
            foreach (var expense in others)
            {
                //如果這筆跟我無關就跳過
                var sep = expense.SplitExpenseParticipants.FirstOrDefault(sep => sep.UserId == userId);
                if (sep == null) continue;
                var member = members.First(m => m.UserId == expense.PayerId);
                member.Balance -= sep.Amount;
                if (sep.IsPaid == false)
                    member.IsPaidBalance -= sep.Amount;
            }
            return members;
        }
    }
}
