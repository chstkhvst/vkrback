using Microsoft.AspNetCore.Mvc;
using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;

namespace WebAPI.Controllers
{
    namespace ASPNETCore.API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class AttendanceController : ControllerBase
        {
            private readonly AttendanceService _attendanceService;
            private readonly ILogger<AttendanceController> _logger;

            public AttendanceController(
                AttendanceService attendanceService,
                ILogger<AttendanceController> logger)
            {
                _attendanceService = attendanceService;
                _logger = logger;
            }

            [HttpGet("[action]")]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetAttendanceAll()
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает все посещения");

                return Ok(await _attendanceService.GetAllAsync());
            }

            [HttpGet("[action]/{id}")]
            public async Task<ActionResult<EventAttendanceDTO>> GetAttendanceById(int id)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещение {id}");

                var attendance = await _attendanceService.GetByIdAsync(id);

                if (attendance == null)
                    return NotFound();

                return Ok(attendance);
            }

            [HttpGet("[action]/{userId}")]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetByUserId(string userId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещения пользователя {userId}");

                return Ok(await _attendanceService.GetByUserIdAsync(userId));
            }

            [HttpGet("[action]/{eventId}")]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetAttendanceByEventId(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещения события {eventId}");

                return Ok(await _attendanceService.GetByEventIdAsync(eventId));
            }

            [HttpGet("[action]/{userId}/{eventId}")]
            public async Task<ActionResult<EventAttendanceDTO>> GetByUserAndEvent(string userId, int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещение user:{userId} event:{eventId}");

                var attendance = await _attendanceService.GetByUserAndEventAsync(userId, eventId);

                if (attendance == null)
                    return NotFound();

                return Ok(attendance);
            }

            [HttpGet("[action]/{eventId}")]
            public async Task<ActionResult<int>> CountParticipants(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} считает участников события {eventId}");

                return Ok(await _attendanceService.CountParticipantsAsync(eventId));
            }

            [HttpPost("[action]")]
            public async Task<ActionResult> Create(CreateEventAttendanceDTO dto)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} создает посещение");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var created = await _attendanceService.AddAsync(dto);

                return Ok(created);
            }

            [HttpPut("[action]/{id}")]
            public async Task<ActionResult> Update(int id, EventAttendanceDTO dto)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} обновляет посещение {id}");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != dto.Id)
                    return BadRequest();

                var updated = await _attendanceService.UpdateAsync(dto);

                return Ok(updated);
            }

            [HttpPut("[action]/{eventId}")]
            public async Task<ActionResult> MarkNoShow(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} отмечает неявки на мероприятие {eventId} ");

                try
                {
                    await _attendanceService.MarkNoShowAsync(eventId);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка : {ex.Message}");
                    return StatusCode(500);
                }
            }

            [HttpPut("[action]/{eventId}")]
            public async Task<ActionResult> MarkCancelled(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} отмечает неявки на мероприятие {eventId} ");

                try
                {
                    await _attendanceService.MarkCancelledAsync(eventId);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка : {ex.Message}");
                    return StatusCode(500);
                }
            }

            [HttpDelete("[action]/{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} удаляет посещение {id}");

                try
                {
                    await _attendanceService.DeleteAsync(id);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка удаления посещения {ex.Message}");
                    return Conflict(new { message = ex.Message });
                }
            }

            [HttpDelete("[action]/{id}")]
            public async Task<IActionResult> SoftDelete(int id)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} мягко удаляет посещение {id}");

                try
                {
                    await _attendanceService.SoftDeleteAsync(id);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка soft delete посещения {ex.Message}");
                    return Conflict(new { message = ex.Message });
                }
            }
        }
    }
}
