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
    public class ScheduleDetailsController : ControllerBase
    {
        PinPinContext _context;
        AuthGetuserId _getUserId;
        public ScheduleDetailsController(PinPinContext context, AuthGetuserId getUserId)
        {
            _context = context;
            _getUserId = getUserId;
        }


        // GET: api/ScheduleDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDetail>>> GetScheduleDetails()
        {
            return await _context.ScheduleDetails.ToListAsync();
        }

        #region 取得行程相關資訊
        // GET: api/ScheduleDetails/5
        [HttpGet("{scheduleId}")]
        public async Task<ActionResult> GetScheduleDetail(int scheduleId)
        {
            int? jwtUserId = _getUserId.PinGetUserId(User).Value;
            if (jwtUserId == null)
            {
                return Unauthorized(new { message = "請先登入會員" });
            }


            var scheduleDetailIds = await _context.SchedulePreviews
                .Where(s => s.ScheduleId == scheduleId)
                .Select(s => s.Id)
                .ToListAsync();

            if (!scheduleDetailIds.Any())
            {
                return NoContent();
            }

            try
            {

                var scheduleDetailsList = await _context.ScheduleDetails
                    .Where(detail => scheduleDetailIds.Contains(detail.ScheduleDayId) && detail.IsDeleted != true)
                    .Select(detail => new ScheduleDetailDTO
                    {
                        Id = detail.Id,
                        ScheduleDayId = detail.ScheduleDayId,
                        LocationName = detail.LocationName,
                        Location = detail.Location,
                        StartTime = detail.StartTime,
                        EndTime = detail.EndTime,
                        Lat = detail.Lat,
                        Lng = detail.Lng,
                        Sort = detail.Sort,
                        Transportations = _context.Transportations
                            .Where(t => t.ScheduleDetailsId == detail.Id)
                            .Select(t => new TransportationDTO
                            {
                                Id = t.Id,
                                ScheduleDetailsId = t.ScheduleDetailsId,
                                TransportationCategoryId = t.TransportationCategoryId
                            }).ToList()
                    }).ToListAsync();

                if (!scheduleDetailsList.Any())
                {
                    return NoContent();
                }

                var scheduleDetailsDict = scheduleDetailsList
                    .GroupBy(detail => detail.ScheduleDayId)
                    .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(detail => detail.Sort).ToList()
                );

                return Ok(new
                {
                    ScheduleDetails = scheduleDetailsDict
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("GetScheduleDetail failed", e);
                return StatusCode(500, new { message = "取得行程相關資料失敗", error = e.Message });
            }
        }
        #endregion

        #region 刪除行程
        // PUT: api/ScheduleDetails/5/6
        [HttpPut("{id}/{scheduleDayId}")]
        public async Task<IActionResult> PutScheduleDetail(int id, int scheduleDayId)
        {
            int? jwtUserId = _getUserId.PinGetUserId(User).Value;
            if (jwtUserId == null || jwtUserId == 0)
            {
                return Unauthorized(new { message = "請先登入會員" });
            }

            var scheduleDetail = await _context.ScheduleDetails
                .FirstOrDefaultAsync(sd => sd.Id == id && sd.ScheduleDayId == scheduleDayId);
            var transportation = await _context.Transportations
                .FirstOrDefaultAsync(t => t.ScheduleDetailsId == id);

            if (scheduleDetail == null)
            {
                return NotFound(new { message = "找不到行程" });
            }

            scheduleDetail.IsDeleted = true;
            scheduleDetail.ModifiedTime = DateTime.Now;
            scheduleDetail.Sort = null;

            try
            {
                if (transportation != null)
                {
                    _context.Transportations.Remove(transportation);
                }

                _context.ScheduleDetails.Update(scheduleDetail);
                await _context.SaveChangesAsync();
                return Ok(new { message = "刪除成功" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("發生DB並存衝突:", ex);
                return StatusCode(409, new { message = "行程已被同行的小夥伴修改或删除，請稍後再試。" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新行程發生錯誤", ex);
                return StatusCode(500, new { message = "更新行程發生錯誤", error = ex.Message });
            }
        }
        #endregion

        #region 新增行程
        // POST: api/ScheduleDetails
        [HttpPost]
        public async Task<ActionResult> PostScheduleDetail([FromBody] ScheduleDetailDTO scheduleDetailDTO)
        {
            try
            {
                int? jwtUserId = _getUserId.PinGetUserId(User).Value;
                if (jwtUserId == null)
                {
                    return Unauthorized(new { message = "請先登入會員" });
                }

                if (_context.ScheduleDetails.Any(s =>
                    s.ScheduleDayId == scheduleDetailDTO.ScheduleDayId &&
                    s.LocationName == scheduleDetailDTO.LocationName &&
                    s.Location == scheduleDetailDTO.Location &&
                    s.Lat == scheduleDetailDTO.Lat &&
                    s.Lng == scheduleDetailDTO.Lng &&
                    s.IsDeleted != true))
                {
                    Console.WriteLine("Location already exists");
                    return Conflict(new { message = "該景點已在同一天的行程中，是否要要再新增一樣的行程？" });
                }


                var maxSortValue = _context.ScheduleDetails
                    .Where(s => s.ScheduleDayId == scheduleDetailDTO.ScheduleDayId && s.IsDeleted != true)
                    .Max(s => (int?)s.Sort) ?? 0;

                ScheduleDetail scheduleDetail = new ScheduleDetail
                {
                    Id = 0,
                    UserId = jwtUserId.Value,
                    ScheduleDayId = scheduleDetailDTO.ScheduleDayId,
                    LocationName = scheduleDetailDTO.LocationName,
                    Location = scheduleDetailDTO.Location,
                    Lat = scheduleDetailDTO.Lat,
                    Lng = scheduleDetailDTO.Lng,
                    StartTime = scheduleDetailDTO.StartTime,
                    EndTime = scheduleDetailDTO.EndTime,
                    Remark = null,
                    IsDeleted = false,
                    ModifiedTime = DateTime.Now,
                    Sort = maxSortValue + 1,
                };
                _context.ScheduleDetails.Add(scheduleDetail);
                await _context.SaveChangesAsync();
                int scheduleDetailId = scheduleDetail.Id;

                Transportation transportation = new Transportation
                {
                    Id = 0,
                    ScheduleDetailsId = scheduleDetailId,
                    TransportationCategoryId = scheduleDetailDTO.TransportationCategoryId
                };
                _context.Transportations.Add(transportation);
                await _context.SaveChangesAsync();
                ScheduleDetailDTO scheduleDetailDTOResult = new ScheduleDetailDTO
                {
                    Id = scheduleDetailId,
                    ScheduleDayId = scheduleDetail.ScheduleDayId,
                    LocationName = scheduleDetail.LocationName,
                    Location = scheduleDetail.Location,
                    Lat = scheduleDetail.Lat,
                    Lng = scheduleDetail.Lng,
                    StartTime = scheduleDetail.StartTime,
                    EndTime = scheduleDetail.EndTime,
                    Sort = scheduleDetail.Sort,
                    TransportationCategoryId = transportation.TransportationCategoryId,
                    TransportationId = transportation.Id
                };

                return Ok(scheduleDetailDTOResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("新增行程發生錯誤", ex);
                return StatusCode(500, new { message = "新增行程發生錯誤", error = ex.Message });
            }
        }
        #endregion

        #region 重複行程確定要存取
        //POST: api/ScheduleDetails/override-schedule-detail
        [HttpPost("override-schedule-detail")]
        public async Task<ActionResult> OverrideScheduleDetail([FromBody] ScheduleDetailDTO scheduleDetailDTO)
        {
            try
            {
                int? jwtUserId = _getUserId.PinGetUserId(User).Value;

                var maxSortValue = _context.ScheduleDetails
                    .Where(s => s.ScheduleDayId == scheduleDetailDTO.ScheduleDayId && s.IsDeleted != true)
                    .Max(s => (int?)s.Sort) ?? 0;

                ScheduleDetail scheduleDetail = new ScheduleDetail
                {
                    Id = 0,
                    UserId = jwtUserId.Value,
                    ScheduleDayId = scheduleDetailDTO.ScheduleDayId,
                    LocationName = scheduleDetailDTO.LocationName,
                    Location = scheduleDetailDTO.Location,
                    Lat = scheduleDetailDTO.Lat,
                    Lng = scheduleDetailDTO.Lng,
                    StartTime = scheduleDetailDTO.StartTime,
                    EndTime = scheduleDetailDTO.EndTime,
                    Remark = null,
                    IsDeleted = false,
                    ModifiedTime = DateTime.Now,
                    Sort = maxSortValue + 1,
                };
                _context.ScheduleDetails.Add(scheduleDetail);
                await _context.SaveChangesAsync();
                int scheduleDetailId = scheduleDetail.Id;

                Transportation transportation = new Transportation
                {
                    Id = 0,
                    ScheduleDetailsId = scheduleDetailId,
                    TransportationCategoryId = scheduleDetailDTO.TransportationCategoryId
                };
                _context.Transportations.Add(transportation);
                await _context.SaveChangesAsync();
                ScheduleDetailDTO scheduleDetailDTOResult = new ScheduleDetailDTO
                {
                    Id = scheduleDetailId,
                    ScheduleDayId = scheduleDetail.ScheduleDayId,
                    LocationName = scheduleDetail.LocationName,
                    Location = scheduleDetail.Location,
                    Lat = scheduleDetail.Lat,
                    Lng = scheduleDetail.Lng,
                    StartTime = scheduleDetail.StartTime,
                    EndTime = scheduleDetail.EndTime,
                    Sort = scheduleDetail.Sort,
                    TransportationCategoryId = transportation.TransportationCategoryId,
                    TransportationId = transportation.Id
                };

                return Ok(scheduleDetailDTOResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("新增行程發生錯誤", ex);
                return StatusCode(500, new { message = "新增行程發生錯誤", error = ex.Message });
            }

        }
        #endregion


        #region 更改id順序
        #endregion

        // DELETE: api/ScheduleDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleDetail(int id)
        {
            var scheduleDetail = await _context.ScheduleDetails.FindAsync(id);
            if (scheduleDetail == null)
            {
                return NotFound();
            }

            _context.ScheduleDetails.Remove(scheduleDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleDetailExists(int id)
        {
            return _context.ScheduleDetails.Any(e => e.Id == id);
        }
    }
}
