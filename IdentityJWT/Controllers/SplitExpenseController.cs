using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Models.DTO.Expense;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    //時間、幣別
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SplitExpensesController : ControllerBase
    {
        private readonly PinPinContext _context;
        private readonly AuthGetuserId _getUserId;
        public SplitExpensesController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;
        }

        //POST:api/SplitExpenses/GetAllExpense
        [HttpPost("GetAllExpense")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetAllExpense()
        {
            try
            {
                List<ExpenseDTO> ExpenseDTOs = await _context.SplitExpenses
                    .AsNoTracking()
                    .Select(expens => new ExpenseDTO
                    {
                        Id = expens.Id,
                        Name = expens.Name,
                        Schedule = expens.Schedule.Name,
                        Payer = expens.Payer.Name,
                        Currency = expens.Currency.Name,
                        Category = expens.SplitCategory.Category,
                        Amount = expens.Amount,
                        Remark = expens.Remark,
                    }).ToListAsync();


                return Ok(ExpenseDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        /// <summary>
        /// 獲取付款人為user的所有付費表
        /// </summary>
        //Get:api/SplitExpenses/GetUserExpense
        [HttpGet("GetExpense")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetUserExpense()
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            try
            {
                List<ExpenseDTO> ExpenseDTOs = await _context.SplitExpenses
                    .AsNoTracking()
                    .Where(expens => expens.PayerId == userID)
                    .Select(expens => new ExpenseDTO
                    {
                        Id = expens.Id,
                        Name = expens.Name,
                        Schedule = expens.Schedule.Name,
                        Payer = expens.Payer.Name,
                        Currency = expens.Currency.Name,
                        Category = expens.SplitCategory.Category,
                        Amount = expens.Amount,
                        Remark = expens.Remark,
                    }).ToListAsync();


                return Ok(ExpenseDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        /// <summary>
        /// 獲取所有scheduleId行程內的分帳表
        /// </summary>
        //Get:api/SplitExpenses/GetScheduleIdExpense{scheduleId}
        [HttpGet("GetScheduleIdExpense{scheduleId}")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetScheduleIdExpense(int scheduleId)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無此行程表
            bool hasSchedule = await _context.Schedules.AnyAsync(sc => sc.Id == scheduleId);
            if (!hasSchedule) return NotFound("Not found schedule");

            //檢查使用者有無在此行程
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == scheduleId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            try
            {
                List<ExpenseDTO> ExpenseDTOs = await _context.SplitExpenses
                    .AsNoTracking()
                    .Where(expens => expens.ScheduleId == scheduleId)
                    .Select(expens => new ExpenseDTO
                    {
                        Id = expens.Id,
                        Name = expens.Name,
                        Schedule = expens.Schedule.Name,
                        Payer = expens.Payer.Name,
                        Currency = expens.Currency.Name,
                        Category = expens.SplitCategory.Category,
                        Amount = expens.Amount,
                        Remark = expens.Remark,
                    }).ToListAsync();


                return Ok(ExpenseDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        /// <summary>
        /// 獲取某個行程的使用者與所有團員之間的分帳關西
        /// </summary>
        //Get:api/SplitExpenses/GetBalance{scheduleId}
        [HttpGet("GetBalance{scheduleId}")]
        public async Task<ActionResult<IEnumerable<ExpenseBalanceDTO>>> GetBalance(int scheduleId)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無此行程表
            bool hasSchedule = await _context.Schedules.AnyAsync(sc => sc.Id == scheduleId);
            if (!hasSchedule) return NotFound("Not found schedule");

            //檢查使用者有無在此行程
            bool isInSchedule = await _context.ScheduleGroups.AnyAsync(sg => sg.ScheduleId == scheduleId && sg.UserId == userID);
            if (!isInSchedule) return Forbid("You can't search not your group");

            try
            {
                List<SplitExpense> expenseList = await _context.SplitExpenses.Where(se => se.ScheduleId == scheduleId)
                                                                             .Include(se => se.SplitExpenseParticipants)
                                                                             .AsNoTracking()
                                                                             .ToListAsync();
                List<ExpenseBalanceDTO> members = await _context.ScheduleGroups.Where(sg => sg.ScheduleId == scheduleId)
                                                                               .Include(sg => sg.User)
                                                                               .Select(sg => new ExpenseBalanceDTO
                                                                               {
                                                                                   UserName = sg.User.Name,
                                                                                   UserId = sg.UserId,
                                                                               }).ToListAsync();
                members.Remove(members.First(m => m.UserId == userID));
                var ec = new ExpenseCalculator();
                var d = ec.CalculateBalance(expenseList, members, (int)userID);
                return Ok(d);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        /// <summary>
        /// 獲取某個行程的使用者與某個團員之間的分帳關西
        /// </summary>
        //api/SplitExpenses/GetUserExpense{scheduleId}&{memberId}
        [HttpGet("GetUserExpense{scheduleId}&{memberId}")]
        public async Task<ActionResult<IEnumerable<ExpenseUserDTO>>> GetUserExpense(int scheduleId, int memberId)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查是否自己搜尋自己
            if (memberId == userID)
                return BadRequest("You can't search yourself");

            //檢查有無此行程表
            bool hasSchedule = await _context.Schedules.AnyAsync(sc => sc.Id == scheduleId);
            if (!hasSchedule) return NotFound("Not found schedule");

            //檢查使用者和團員有無在此行程
            bool areBothInSchedule = await _context.ScheduleGroups
                                       .Where(sg => sg.ScheduleId == scheduleId)
                                       .CountAsync(sg => sg.UserId == userID || sg.UserId == memberId) == 2;

            if (!areBothInSchedule) return Forbid("You or member not in group");

            try
            {
                //找所有跟使用者與指定團員的分帳表
                var expenseList = await _context.SplitExpenses
                                        .Where(se => se.ScheduleId == scheduleId)
                                        .Where(se => se.PayerId == userID && se.SplitExpenseParticipants.Any(sep => sep.UserId == memberId) || (se.PayerId == memberId && se.SplitExpenseParticipants.Any(sep => sep.UserId == userID)))
                                        .Include(se => se.SplitExpenseParticipants)
                                        .Include(se => se.SplitCategory)
                                        .AsNoTracking()
                                        .ToListAsync();

                List<ExpenseUserDTO> expenseUsers = new List<ExpenseUserDTO>();
                foreach (var expense in expenseList)
                {
                    ExpenseUserDTO dto = new ExpenseUserDTO();
                    dto.ExpenseId = expense.Id;
                    if (expense.PayerId == userID)
                    {
                        dto.ExpenseName = expense.Name;
                        dto.ExpenseCategory = expense.SplitCategory.Category;
                        //!!!!!!!!!!!!!代辦幣別
                        dto.Amount = expense.SplitExpenseParticipants.First(sep => sep.UserId == memberId).Amount;
                        dto.IsPaid = expense.SplitExpenseParticipants.First(sep => sep.UserId == memberId).IsPaid;
                    }
                    else if (expense.PayerId == memberId)
                    {
                        dto.ExpenseName = expense.Name;
                        dto.ExpenseCategory = expense.SplitCategory.Category;
                        //!!!!!!!!!!!!!代辦幣別
                        dto.Amount = expense.SplitExpenseParticipants.First(sep => sep.UserId == userID).Amount;
                        dto.Amount = dto.Amount > 0 ? dto.Amount * -1 : throw new Exception("LogicError");
                        dto.IsPaid = expense.SplitExpenseParticipants.First(sep => sep.UserId == userID).IsPaid;
                    }
                    else
                    {
                        throw new Exception("LogicError");
                    }
                    expenseUsers.Add(dto);
                }
                return Ok(expenseUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        /// <summary>
        /// 獲取指定分帳表的主表和所有明細
        /// </summary>
        //Get:api/SplitExpenses/GetExpense{id}
        [HttpGet("GetExpense{id}")]
        public async Task<ActionResult<ExpenseDTO>> GetExpense(int id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            bool isExist = _context.SplitExpenses.Any(se => se.Id == id);
            if (!isExist) return NotFound("Not expense found for this id");

            //確認要查詢表的行程團是否有user
            List<int> userIds = await _context.SplitExpenses
                .Where(se => se.Id == id)
                .Include(se => se.Schedule)
                .ThenInclude(s => s.ScheduleGroups)
                .SelectMany(se => se.Schedule.ScheduleGroups.Select(sg => sg.UserId))
                .ToListAsync();
            if (!userIds.Contains(userID.Value)) return Forbid("You can't search not your group");

            try
            {
                SplitExpense expense = _context.SplitExpenses
                    .Include(se => se.Schedule)
                    .Include(se => se.Payer)
                    .Include(se => se.SplitCategory)
                    .Include(se => se.Currency)
                    .Include(se => se.SplitExpenseParticipants)
                    .ThenInclude(sep => sep.User)
                    .First(se => se.Id == id);
                ExpenseDTO dto = new ExpenseDTO
                {
                    Id = expense.Id,
                    Name = expense.Name,
                    Schedule = expense.Schedule.Name,
                    Payer = expense.Payer.Name,
                    Category = expense.SplitCategory.Category,
                    Currency = expense.Currency.Code,
                    Amount = expense.Amount,
                    Remark = expense.Remark,
                    CreatedAt = expense.CreatedAt.HasValue ? expense.CreatedAt.Value.ToString("f") : "查無時間",
                    ExpenseParticipants = expense.SplitExpenseParticipants.Select(sep => new ExpenseParticipantDTO
                    {
                        UserId = sep.UserId,
                        UserName = sep.User.Name,
                        Amount = sep.Amount,
                        IsPaid = sep.IsPaid,
                    }).ToList(),
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //POST:api/SplitExpenses/PostExpense
        [HttpPost("PostExpense")]
        public async Task<ActionResult> PostExpense([FromBody] CreateNewExpensedDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //驗證傳入模型是否正確
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList()
                );

                return BadRequest(new { Error = errors });

            }

            var groupUserList = await _context.ScheduleGroups
                .Where(group => group.ScheduleId == dto.ScheduleId)
                .Select(group => group.UserId)
                .ToListAsync();

            List<int> users = dto.Participants.Select(participant => participant.UserId).ToList();

            //檢查所有傳入值是吼有問題
            //檢查是否有重複的使用者
            if (users.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return BadRequest("There are duplicate users.");
            }
            //檢查是否全部都有在群組裡
            if ((!users.All(user => groupUserList.Contains(user)))||!groupUserList.Contains(dto.PayerId))
            {
                return BadRequest("Some users are not in the group.");
            }
            //檢查費用表種類
            bool splitCategoryExists = await _context.SplitCategories.AnyAsync(category => category.Id == dto.SplitCategoryId);
            if (!splitCategoryExists)
            {
                return BadRequest("Invalid SplitCategory.");
            }
            //檢查幣別
            bool currencyCategoryExists = await _context.CostCurrencyCategories.AnyAsync(category => category.Id == dto.CurrencyId);
            if (!currencyCategoryExists)
            {
                return BadRequest("Invalid CostCurrencyCategory.");
            }
            //檢查費表是否有名稱
            if (string.IsNullOrEmpty(dto.Name))
            {
                return BadRequest("Name cannot be empty.");
            }
            //檢查金額是否為正
            if (dto.Amount <= 0)
            {
                return BadRequest("Main amount must be greater than zero.");
            }
            //檢查費用表明細金額
            if (dto.Participants.Any(participant => participant.Amount < 0))
            {
                return BadRequest("Each participant's amount must be greater than zero.");
            }

            //檢查明細金額相加是否等於總金額
            float total = dto.Participants.Sum(participant => participant.Amount);
            if (dto.Amount != total)
            {
                return BadRequest("The total amount of participants does not match the main amount.");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                SplitExpense? splitExpense = new SplitExpense
                {
                    ScheduleId = dto.ScheduleId,
                    PayerId = dto.PayerId,
                    SplitCategoryId = dto.SplitCategoryId,
                    Name = dto.Name,
                    CurrencyId = dto.CurrencyId,
                    Amount = dto.Amount,
                    Remark = dto.Remark,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.SplitExpenses.Add(splitExpense);
                await _context.SaveChangesAsync();

                var userDictionary = await _context.ScheduleGroups
                    .Where(group => group.ScheduleId == splitExpense.ScheduleId)
                    .Include(group => group.User)
                    .Where(group => group.User != null)
                    .ToDictionaryAsync(group => group.User.Name, group => group.User.Id);

                var participants = new List<SplitExpenseParticipant>();
                foreach (var item in dto.Participants)
                {
                    if (!userDictionary.TryGetValue(item.UserName, out var userId))
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Error = $"User {item.UserName} not found." });
                    }

                    var participant = new SplitExpenseParticipant
                    {
                        SplitExpenseId = splitExpense.Id,
                        UserId = userId,
                        Amount = item.Amount,
                        IsPaid = item.UserId == splitExpense.PayerId ? true : item.IsPaid,
                    };
                    participants.Add(participant);
                }

                _context.SplitExpenseParticipants.AddRange(participants);

                splitExpense = await _context.SplitExpenses
                    .Include(expense => expense.Schedule)
                    .Include(expense => expense.Payer)
                    .Include(expense => expense.SplitCategory)
                    .Include(expense => expense.Currency)
                    .FirstOrDefaultAsync(expense => expense.Id == splitExpense.Id);

                if (splitExpense == null)
                {
                    return NotFound("Split expense not found.");
                }

                ExpenseDTO ExpenseDTO = new ExpenseDTO
                {
                    Schedule = splitExpense.Schedule.Name,
                    Name = splitExpense.Name,
                    Payer = splitExpense.Payer.Name,
                    Category = splitExpense.SplitCategory.Category,
                    Currency = splitExpense.Currency.Code,
                    Amount = splitExpense.Amount,
                    Remark = splitExpense.Remark,
                };

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetExpense), new { id = splitExpense.PayerId }, ExpenseDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense.", Details = ex.Message });
            }
        }

        //PUT: api/SplitExpenses/UpdateExpense{id}
        [HttpPut("UpdateExpense{id}")]
        public async Task<ActionResult> UpdateExpense(int id, [FromBody] CreateNewExpensedDTO dto)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            SplitExpense? splitExpense = await _context.SplitExpenses.FirstOrDefaultAsync(expense => expense.Id == id);
            if (splitExpense == null)
            {
                return NotFound("Not found this id");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList()
                );

                return BadRequest(new { Error = errors });
            }

            var groupUserList = await _context.ScheduleGroups
                .Where(group => group.ScheduleId == splitExpense.ScheduleId)
                .Select(group => group.UserId)
                .ToListAsync();

            List<int> users = dto.Participants.Select(participant => participant.UserId).ToList();

            //檢查所有傳入值是吼有問題
            //檢查是否有重複的使用者
            if (users.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return BadRequest("There are duplicate users.");
            }
            //檢查是否全部都有在群組裡
            if ((!users.All(user => groupUserList.Contains(user))) || !groupUserList.Contains(dto.PayerId))
            {
                return BadRequest("Some users are not in the group.");
            }

            //檢查費用表種類
            bool splitCategoryExists = await _context.SplitCategories.AnyAsync(category => category.Id == dto.SplitCategoryId);
            if (!splitCategoryExists)
            {
                return BadRequest("Invalid SplitCategory.");
            }

            //檢查幣別
            bool currencyCategoryExists = await _context.CostCurrencyCategories.AnyAsync(category => category.Id == dto.CurrencyId);
            if (!currencyCategoryExists)
            {
                return BadRequest("Invalid CostCurrencyCategory.");
            }

            //檢查費表是否有名稱
            if (string.IsNullOrEmpty(dto.Name))
            {
                return BadRequest("Name cannot be empty.");
            }

            //檢查金額是否為正
            if (dto.Amount <= 0)
            {
                return BadRequest("Main amount must be greater than zero.");
            }

            //檢查費用表明細金額
            if (dto.Participants.Any(participant => participant.Amount <= 0))
            {
                return BadRequest("Each participant's amount must be greater than zero.");
            }

            //檢查明細金額相加是否等於總金額
            float total = dto.Participants.Sum(participant => participant.Amount);
            if (dto.Amount != total)
            {
                return BadRequest("The total amount of participants does not match the main amount.");
            }

            List<ExpenseParticipantDTO> participantDTOs = dto.Participants.ToList();
            //修改資料庫
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<SplitExpenseParticipant> participants = await _context.SplitExpenseParticipants
                                                .Where(ep => ep.SplitExpenseId == id)
                                                .ToListAsync();

                splitExpense.PayerId = dto.PayerId;
                splitExpense.SplitCategoryId = dto.SplitCategoryId;
                splitExpense.Name = dto.Name;
                splitExpense.CurrencyId = dto.CurrencyId;
                splitExpense.Amount = dto.Amount;
                splitExpense.Remark = dto.Remark;

                _context.SplitExpenses.Update(splitExpense); ;

                _context.SplitExpenseParticipants.RemoveRange(participants);
                await _context.SaveChangesAsync();

                foreach (var epdto in participantDTOs)
                {
                    var newParticipant = new SplitExpenseParticipant
                    {
                        SplitExpenseId = id,
                        UserId = epdto.UserId,
                        Amount = epdto.Amount,
                        IsPaid = epdto.IsPaid
                    };

                    _context.SplitExpenseParticipants.Add(newParticipant);
                };

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "An error occurred while updating the Expense.", Details = ex.Message });
            }
        }

        //Delete:api/SplitExpenses/DeleteExpense{id}
        [HttpDelete("DeleteExpense{id}")]
        public async Task<ActionResult> DeleteExpense(int id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //檢查有無此筆資料
            SplitExpense? splitExpense = await _context.SplitExpenses.FirstOrDefaultAsync(se => se.Id == id);
            if (splitExpense == null) return BadRequest("Not found Expense");

            //檢查是否有權限更改
            var groupUserList = await _context.ScheduleGroups.Where(group => group.ScheduleId == splitExpense.ScheduleId)
                                                             .Select(group => group.UserId)
                                                             .ToListAsync();
            if (!groupUserList.Contains(userID.Value))
                return BadRequest("User does not have permission to delete this expense.");

            try
            {
                _context.Remove(splitExpense);
                await _context.SaveChangesAsync();

                return Ok("Delete Success");
            }
            catch
            {
                return StatusCode(500, "A Database error.");
            }
        }
    }
}
