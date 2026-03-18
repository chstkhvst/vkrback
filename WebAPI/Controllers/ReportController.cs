using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetReports()
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает список жалоб");

            var reports = await _reportService.GetAllAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
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

        [HttpGet("sender/{senderId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetBySender(string senderId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы отправителя {senderId}");

            var reports = await _reportService.GetBySenderIdAsync(senderId);
            return Ok(reports);
        }

        [HttpGet("reported/{reportedId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetByReported(string reportedId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы на пользователя {reportedId}");

            var reports = await _reportService.GetByReportedIdAsync(reportedId);
            return Ok(reports);
        }

        [HttpGet("status/{statusId}")]
        public async Task<ActionResult<IEnumerable<UserReportDTO>>> GetByStatus(int statusId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает жалобы со статусом {statusId}");

            var reports = await _reportService.GetByStatusIdAsync(statusId);
            return Ok(reports);
        }

        [HttpPost]
        public async Task<ActionResult<UserReportDTO>> CreateReport(CreateReportDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} создает жалобу");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _reportService.AddAsync(dto);

            return CreatedAtAction(nameof(GetReportById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserReportDTO>> UpdateReport(int id, UserReportDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} обновляет жалобу {id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest();

            var updated = await _reportService.UpdateAsync(dto);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
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

        [HttpDelete("soft/{id}")]
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