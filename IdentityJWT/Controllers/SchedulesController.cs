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
    public class SchedulesController : ControllerBase
    {
        PinPinContext _context;
        AuthGetuserId _getUserId;
        public SchedulesController(PinPinContext context, AuthGetuserId getuserId)
        {
            _context = context;
            _getUserId = getuserId;

        }

        #region 進入edit畫面需要行程資料(userid==jwtuserid || (schdule.userid != jwtuserid && authority == 2 )
        // GET: api/Schedules/Entereditdetailsch
        [HttpGet("Entereditdetailsch/{scheduleId}")]
        public async Task<IActionResult> Entereditdetailsch(int scheduleId)
        {
            //IEnumerable<ScheduleDTO> schedules = Enumerable.Empty<ScheduleDTO>();
            try
            {
                int jwtuserID = _getUserId.PinGetUserId(User).Value;
                if (jwtuserID == 0)
                {
                    return Unauthorized(new { message = "請先登入會員" });
                }
                var scheduledetails = await _context.Schedules
                .Where(s => s.Id == scheduleId)
                .Include(s => s.User)
                .Include(s => s.ScheduleGroups)
                    .ThenInclude(sg => sg.User)
                .Include(s => s.ScheduleAuthorities)
                .Select(s => new ScheduleDTO
                {
                    Id = scheduleId,
                    HostId = s.UserId,
                    Name = s.Name,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    CreatedAt = s.CreatedAt,
                    Picture = s.Picture,
                    PlaceId = s.PlaceId,
                    isHost = s.ScheduleGroups.Select(S => S.IsHoster).FirstOrDefault(),
                    lng = s.Lng,
                    lat = s.Lat,
                    SharedUserIDs = s.ScheduleGroups
                        .Select(sg => (int?)sg.UserId)
                        .ToList(),
                    canedittitle = s.ScheduleAuthorities.Where(sa => sa.ScheduleId == scheduleId && sa.UserId == jwtuserID).All(sa => sa.AuthorityCategoryId == 8),
                    caneditdetail = s.ScheduleAuthorities.Where(sa => sa.ScheduleId == scheduleId && sa.UserId == jwtuserID).Any(sa => (sa.AuthorityCategoryId == 2) || (sa.AuthorityCategoryId == 8))

                }).ToListAsync();

                if (scheduledetails == null || !scheduledetails.Any())
                {

                    Console.WriteLine("查無相關紀錄");
                    return NoContent();
                }

                var Schedule_Day_Ids = await _context.SchedulePreviews
                .Where(sp => sp.ScheduleId == scheduleId)
                .ToDictionaryAsync(sp => sp.Id, sp => sp.Date);


                return Ok(new { ScheduleDetail = scheduledetails, SceduleDateIdInfo = Schedule_Day_Ids });

            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(409, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                throw new Exception("伺服器發生錯誤，請稍後再試");
            }

        }
        #endregion

        #region 讀取user的所有行程
        // GET: api/Schedules/AllSchedules
        [HttpGet("AllSchedules")]
        public async Task<IActionResult> GetAllUserSchedules()
        {
            List<ScheduleDTO> allSchedules = new List<ScheduleDTO>();
            try
            {
                int userID = _getUserId.PinGetUserId(User).Value;
                if (userID == 0)
                {
                    return Unauthorized(new { message = "請先登入會員" });
                }

                var mainSchedules = await _context.Schedules
                    .Where(s => s.UserId == userID)
                    .Include(s => s.User)
                    .Include(s => s.ScheduleGroups)
                    .Select(s => new ScheduleDTO
                    {
                        Id = s.Id,
                        HostId = s.UserId,
                        Name = s.Name,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        CreatedAt = s.CreatedAt,
                        UserName = s.User.Name,
                        Picture = s.Picture,
                        PlaceId = s.PlaceId,
                        lng = s.Lng,
                        lat = s.Lat,
                        canedittitle = true,
                        caneditdetail = true,
                        isHost = true,
                        SharedUserIDs = s.ScheduleGroups.Select(s => (int?)s.UserId).ToList(),
                        SharedUserNames = s.ScheduleGroups.Select(s => (string?)s.User.Name).Distinct().ToList(),
                    }).ToListAsync();

                // 獲取用戶參加的行程（不是主辦者）
                var groupScheduleIds = await _context.ScheduleGroups
                    .Where(sg => sg.UserId == userID && sg.IsHoster == false && !sg.LeftDate.HasValue)
                    .Select(sg => sg.ScheduleId)
                    .Distinct()
                    .ToListAsync();

                var groupSchedules = await _context.Schedules
                    .Where(s => groupScheduleIds.Contains(s.Id))
                    .Include(s => s.User)
                    .Include(s => s.ScheduleGroups)
                    .ThenInclude(sg => sg.User)
                    .Where(s => !s.ScheduleGroups.Any(sg => sg.LeftDate.HasValue))
                    .Select(s => new ScheduleDTO
                    {
                        Id = s.Id,
                        HostId = s.UserId,
                        Name = s.Name,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        CreatedAt = s.CreatedAt,
                        UserName = s.User.Name,
                        Picture = s.Picture,
                        PlaceId = s.PlaceId,
                        lng = s.Lng,
                        lat = s.Lat,
                        isHost = false,
                        caninvited = s.ScheduleAuthorities.All(sa => sa.AuthorityCategoryId == 2),
                        canedittitle = false,
                        caneditdetail = s.ScheduleGroups.All(s => s.ScheduleId == s.Id && s.LeftDate == null && s.UserId == userID) && s.ScheduleAuthorities.All(sa => sa.ScheduleId == s.Id && (sa.AuthorityCategoryId == 2 || sa.AuthorityCategoryId == 8)),
                        SharedUserIDs = s.ScheduleGroups.Select(sg => (int?)sg.UserId).ToList(),
                        SharedUserNames = s.ScheduleGroups
                            .Where(sg => sg.UserId != userID)
                            .Select(sg => (string?)sg.User.Name)
                            .Distinct()
                            .ToList(),
                    }).ToListAsync();


                if (mainSchedules.Any() && groupSchedules.Any())
                {
                    return Ok(new { MainSchedules = mainSchedules, GroupSchedules = groupSchedules });
                }
                else if (mainSchedules.Any() && !groupSchedules.Any())
                {
                    return Ok(new { GroupSchedules = "目前沒有參加的旅遊群組", MainSchedules = mainSchedules });
                }
                else if (!mainSchedules.Any() && groupSchedules.Any())
                {

                    return Ok(new { MainSchedules = "趕快規劃新行程吧", GroupSchedules = groupSchedules });
                }
                else
                {
                    return NotFound(new { message = "尚無新創的旅遊行程及參與的旅遊群組" });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, "伺服器發生錯誤，請稍後再試");
            }
        }
        #endregion

        #region 查詢行程_2024/8/8-暫不採用該功能
        //Get:api/Schedules/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetUserSpecifiedSch(string name)
        {
            IEnumerable<ScheduleDTO> schedules = Enumerable.Empty<ScheduleDTO>();
            try
            {
                int userID = _getUserId.PinGetUserId(User).Value;
                schedules = await _context.Schedules
                    .Where(s => s.UserId == userID && s.Name.Contains(name))
                    .Join(
                        _context.Users,
                        sch => sch.UserId,
                        usr => usr.Id,
                        (sch, usr) => new ScheduleDTO
                        {
                            Id = sch.Id,
                            HostId = sch.UserId,
                            Name = sch.Name,
                            StartTime = sch.StartTime,
                            EndTime = sch.EndTime,
                            CreatedAt = sch.CreatedAt,
                            UserName = usr.Name,
                            SharedUserNames = sch.ScheduleGroups.Select(sg => (string?)sg.User.Name).Distinct().ToList(),
                            lng = sch.Lng,
                            lat = sch.Lat,
                        }).ToListAsync();

                if (schedules == null || !schedules.Any())
                {
                    return NotFound();
                }
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"處理請求時發生錯誤: {ex.Message}");
                return BadRequest();
            }
        }
        #endregion

        #region 修改自行創建主題
        // PUT: api/Schedule/UpdateSchedule/{scheduleId}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("UpdateSchedule/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleDTO schDTO, int scheduleId)
        {
            int jwtuserID = _getUserId.PinGetUserId(User).Value;

            // 查找 Schedule
            Schedule? sch = await _context.Schedules.FindAsync(scheduleId);

            if (sch == null)
            {
                return NotFound(new { message = "找不到該行程" });
            }

            // 更新 Schedule 主表
            sch.Id = scheduleId;
            sch.Name = schDTO.Name;
            sch.StartTime = schDTO.StartTime;
            sch.EndTime = schDTO.EndTime;
            sch.UserId = jwtuserID;
            sch.CreatedAt = DateTime.Now;
            _context.Entry(sch).State = EntityState.Modified;

            #region 要記得把變更行程日期的功能完善(2024/8/16暫不做)
            //var existingPreviews = _context.SchedulePreviews
            //    .Where(sp => sp.ScheduleId == scheduleId)
            //    .ToList();

            //_context.SchedulePreviews.RemoveRange(existingPreviews);

            //// 生成新的日期列表并重新添加 SchedulePreview 记录
            //var newDates = Enumerable.Range(0, (schDTO.EndTime.ToDateTime(new TimeOnly()) - schDTO.StartTime.ToDateTime(new TimeOnly())).Days + 1)
            //                  .Select(offset => schDTO.StartTime.AddDays(offset))
            //                  .ToList();

            //foreach (var date in newDates)
            //{
            //    _context.SchedulePreviews.Add(new SchedulePreview
            //    {
            //        ScheduleId = scheduleId,
            //        Date = date,
            //        CreatedAt = DateTime.Now
            //    });
            //}
            #endregion
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "行程已更新" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(scheduleId))
                {
                    return BadRequest("系统发生错误");
                }
                throw;
            }
        }

        #endregion

        #region 自行創建主題
        // POST: api/Schedules
        [HttpPost]
        public async Task<IActionResult> PostSchedule([FromBody] EditScheduleDTO editschDTO)
        {
            try
            {
                if (editschDTO == null)
                {
                    return NotFound();
                }

                int userID = _getUserId.PinGetUserId(User).Value;

                decimal? lat = null;
                decimal? lng = null;
                if (!string.IsNullOrEmpty(editschDTO.Lat))
                {
                    if (decimal.TryParse(editschDTO.Lat, out var parsedLat))
                    {
                        lat = parsedLat;
                    }
                    else
                    {
                        return BadRequest("Invalid latitude format.");
                    }
                }

                if (!string.IsNullOrEmpty(editschDTO.Lng))
                {
                    if (decimal.TryParse(editschDTO.Lng, out var parsedLng))
                    {
                        lng = parsedLng;
                    }
                    else
                    {
                        return BadRequest("Invalid longitude format.");
                    }
                }

                var schedule = new Schedule
                {
                    Id = 0,
                    Name = editschDTO.Name,
                    StartTime = editschDTO.StartTime,
                    EndTime = editschDTO.EndTime,
                    CreatedAt = DateTime.Now,
                    UserId = userID,
                    Lng = lng,
                    Lat = lat,
                    PlaceId = editschDTO.PlaceId,
                    Picture = editschDTO.Pictureurl,
                };

                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();

                int newScheduleId = schedule.Id;
                var schedulegroup = new ScheduleGroup
                {
                    Id = 0,
                    ScheduleId = newScheduleId,
                    UserId = userID,
                    IsHoster = true,
                };

                // 添加 ScheduleGroup 並保存更改
                _context.ScheduleGroups.Add(schedulegroup);
                await _context.SaveChangesAsync();

                // 創建 ScheduleAuthority 物件
                var scheduleAuthority = new ScheduleAuthority
                {
                    Id = 0,
                    ScheduleId = newScheduleId,
                    UserId = userID,
                    AuthorityCategoryId = 8
                };

                // 添加 ScheduleAuthority 並保存更改
                _context.ScheduleAuthorities.Add(scheduleAuthority);
                await _context.SaveChangesAsync();
                //計算總共有幾天
                var editschDTOStartTime = editschDTO.StartTime.ToString();
                var editschDTOEndTime = editschDTO.EndTime.ToString();
                int dayamounts = (int)(DateTime.Parse(editschDTOEndTime) - DateTime.Parse(editschDTOStartTime)).TotalDays + 1;
                SchedulePreview schedulePreview = new SchedulePreview();
                for (int i = 0; i < dayamounts; i++)
                {
                    schedulePreview = new SchedulePreview
                    {
                        Id = 0,
                        ScheduleId = newScheduleId,
                        Date = editschDTO.StartTime.AddDays(i),
                        CreatedAt = DateTime.Now
                    };
                    _context.SchedulePreviews.Add(schedulePreview);
                }
                await _context.SaveChangesAsync();

                return Ok(new { message = "行程創建成功" }); // 返回成功響應

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "伺服器發生錯誤，請稍後再試");
            }
        }
        #endregion

        #region 刪除行程主題(user自行創建的)
        // DELETE: api/Schedules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            try
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception occurred: {ex.Message}");
                return StatusCode(500, "刪除失敗!"); // 500 Internal Server Error if deletion fails
            }
        }
        #endregion

        #region bool行程是否存在
        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
        #endregion

        #region 回傳與這個使用者有關的行程表 型態Dictionary<int, string>>
        //GET:api/schedules/GetRelatedSchedules
        //GET資料回傳為Dictionary<ScheduleId,ScheduleName>
        [HttpGet("GetRelatedSchedules")]
        public async Task<ActionResult<Dictionary<int, string>>> GetRelatedSchedules()
        {
            int userID = _getUserId.PinGetUserId(User).Value;
            try
            {
                Dictionary<int, string> scheduleDictionary = await _context.ScheduleGroups
                    .Where(group => group.UserId == userID)
                    .Include(group => group.Schedule)
                    .ToDictionaryAsync(group => group.ScheduleId, group => group.Schedule.Name);
                if (scheduleDictionary.Count == 0) return NotFound("Not found about your schedles");
                return Ok(scheduleDictionary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        #endregion


    }
}
