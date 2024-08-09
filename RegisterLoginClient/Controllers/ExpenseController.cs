using Microsoft.AspNetCore.Mvc;

namespace PinPinClient.Controllers
{
    public class ExpenseController : Controller
    {
        //GET:Expense/ModalExpense
        public IActionResult ExpenseModal()
        {
            return PartialView("_ExpenseModalPartial");
        }

        //GET:Expense/CreatExpense
        public IActionResult CreatExpense()
        {
            return PartialView("_CreateExpensePartial");
        }
    }
}
