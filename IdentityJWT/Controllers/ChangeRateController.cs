using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinPinServer.Models;
using PinPinServer.Models.DTO;
using PinPinServer.Services;

namespace PinPinServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChangeRateController : ControllerBase
    {
        private readonly ChangeRateService _rateService;
        private readonly AuthGetuserId _getUserId;
        private readonly PinPinContext _context;
        public ChangeRateController(ChangeRateService changeRateService, AuthGetuserId getuserId, PinPinContext context)
        {
            _rateService = changeRateService;
            _getUserId = getuserId;
            _context = context;
        }

        //GET:api/ChangeRate/{code}
        [HttpGet("{code}")]
        public async Task<ActionResult<List<ChangeRateDTO>>> Get(string code)
        {
            int? userID = _getUserId.PinGetUserId(User).Value;
            if (userID == null || userID == 0) return BadRequest("Invalid user ID");

            List<CostCategoryDTO> dtos = _context.CostCurrencyCategories.Where(ccc => ccc.IsDeleted == false).Select(ccc => new CostCategoryDTO
            {
                Id = ccc.Id,
                Code = ccc.Code,
                Name = ccc.Name,
                Icon = ccc.Icon,
            }).AsNoTracking().ToList();

            if (!dtos.Any(dto => dto.Code == code)) return BadRequest("Not has this Code");

            var result = await _rateService.GetChangeRate(code, dtos);
            if (result.Count>=0) return Ok(await _rateService.GetChangeRate(code, dtos));
            else return NotFound("Not support this code");
        }
    }
}
