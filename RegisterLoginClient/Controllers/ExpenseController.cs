using Microsoft.AspNetCore.Mvc;

namespace PinPinClient.Controllers
{
    public class ExpenseController : Controller
    {
        //GET:Expense/Index
        public IActionResult Index()
        {
            return View();
        }

        //GET:Expense/LoadExpense
        public IActionResult LoadExpense()
        {
            return PartialView("_expensePartial");
        }

        //GET:Expense/LoadCreate
        public IActionResult LoadCreate()
        {
            return PartialView("_createPartial");
        }
    }
}
