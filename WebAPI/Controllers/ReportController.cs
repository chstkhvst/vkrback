using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ReportGroupDTO>>> GetGroupedReports([FromQuery] int? statusId, [FromQuery] string? keywords)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает сгруппированные жалобы (statusId: {statusId})");

            var groups = await _reportService.GetGroupedReportsAsync(statusId, keywords);

            return Ok(groups);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetReports()
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает список жалоб");

            var reports = await _reportService.GetAllAsync();
            return Ok(reports);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<UserReportDTO>> GetReportById(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобу {id}");

            var report = await _reportService.GetByIdAsync(id);

            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpGet("[action]/{senderId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetReportBySenderId(string senderId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы отправителя {senderId}");

            var reports = await _reportService.GetBySenderIdAsync(senderId);
            return Ok(reports);
        }

        [HttpGet("[action]/{reportedId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetReportByReportedId(string reportedId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы на пользователя {reportedId}");

            var reports = await _reportService.GetByReportedIdAsync(reportedId);
            return Ok(reports);
        }

        [HttpGet("[action]/{statusId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetByStatus(int statusId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы со статусом {statusId}");

            var reports = await _reportService.GetByStatusIdAsync(statusId);
            return Ok(reports);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<UserReportDTO>> CreateReport(CreateReportDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                dto.SenderUserId = currentUserId;
            }
            _logger.LogInformation($"{currUser} создает жалобу");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _reportService.AddAsync(dto);

            return Ok(created);
        }
        [HttpPut("[action]/{userId}")]
        public async Task<ActionResult> MarkReportsClosed(string userId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"{currUser} меняет статус всех нерассмотренных жалоб для {userId} ");

            try
            {
                await _reportService.MarkReportsClosedAsync(userId, currentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка : {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult<UserReportDTO>> UpdateReport(int id, UserReportDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} обновляет жалобу {id}");
            var isModerator = User.IsInRole("moderator");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (isModerator)
            {
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    dto.ModeratedByUserId = currentUserId;
                }
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest();

            var updated = await _reportService.UpdateAsync(dto);

            return Ok(updated);
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} удаляет жалобу {id}");

            try
            {
                await _reportService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления жалобы {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> SoftDeleteReport(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} мягко удаляет жалобу {id}");

            try
            {
                await _reportService.SoftDeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка soft delete жалобы {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }
    }
}