using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
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
        /// <returns></returns>
        //Get:api/SplitExpenses/GetExpense
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

        [HttpGet("GetExpense{scheduleId}")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetExpense(int scheduleId)
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

        //POST:api/SplitExpenses/GetExpense_participant
        [HttpPost("GetExpense_participant")]
        public async Task<ActionResult<IEnumerable<ExpenseParticipantDTO>>> GetExpense_participant([FromForm] int Id)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;

            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            //確認要查詢表的行程團是否有user
            List<int> userIds = await _context.SplitExpenses
                .Where(se => se.Id == Id)
                .Include(se => se.Schedule)
                .ThenInclude(s => s.ScheduleGroups)
                .SelectMany(se => se.Schedule.ScheduleGroups.Select(sg => sg.UserId))
                .ToListAsync();
            if (!userIds.Contains(userID.Value)) return Forbid("You can't search not your group");

            try
            {
                List<ExpenseParticipantDTO> participantDTOs = await _context.SplitExpenseParticipants
                    .AsNoTracking()
                    .Where(participant => participant.SplitExpenseId == Id)
                    .Select(participant => new ExpenseParticipantDTO
                    {
                        UserName = participant.User.Name,
                        Amount = participant.Amount,
                        IsPaid = participant.IsPaid,

                    }).ToListAsync();


                return Ok(participantDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //POST:api/SplitExpenses/CreateNewExpense
        [HttpPost("CreateNewExpense")]
        public async Task<ActionResult> CreateNewExpense([FromBody] CreateNewExpensedDTO dto)
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
            users.Add(dto.PayerId);

            //檢查所有傳入值是吼有問題
            //檢查是否有重複的使用者
            if (users.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return BadRequest("There are duplicate users.");
            }
            //檢查是否全部都有在群組裡
            if (!users.All(user => groupUserList.Contains(user)))
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
            decimal total = dto.Participants.Sum(participant => participant.Amount);
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
                        IsPaid = item.IsPaid,
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
                    Currency = splitExpense.Currency.Name,
                    Amount = splitExpense.Amount,
                    Remark = splitExpense.Remark,
                };

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetExpense), new { user_Id = splitExpense.PayerId }, ExpenseDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "An error occurred while creating the CreateNewExpense.", Details = ex.Message });
            }
        }

        // PUT: api/SplitExpenses/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateExpense(int id, [FromBody] CreateNewExpensedDTO dto)
        {
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
            users.Add(dto.PayerId);

            //檢查所有傳入值是吼有問題
            //檢查是否有重複的使用者
            if (users.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return BadRequest("There are duplicate users.");
            }
            //檢查是否全部都有在群組裡
            if (!users.All(user => groupUserList.Contains(user)))
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
            decimal total = dto.Participants.Sum(participant => participant.Amount);
            if (dto.Amount != total)
            {
                return BadRequest("The total amount of participants does not match the main amount.");
            }

            List<SplitExpenseParticipant> participants = await _context.SplitExpenseParticipants
                .Where(ep => ep.SplitExpenseId == id)
                .ToListAsync();

            List<ExpenseParticipantDTO> participantDTOs = dto.Participants.ToList();

            //檢查更改前後的明細數量是否正確
            if (participantDTOs.Count != participants.Count)
                return BadRequest("The number of participants does not match.");

            //修改資料庫
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
    }
}
