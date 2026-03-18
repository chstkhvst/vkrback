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

            [HttpGet]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetAll()
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает все посещения");

                return Ok(await _attendanceService.GetAllAsync());
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<EventAttendanceDTO>> GetById(int id)
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

            [HttpGet("user/{userId}")]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetByUserId(string userId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещения пользователя {userId}");

                return Ok(await _attendanceService.GetByUserIdAsync(userId));
            }

            [HttpGet("event/{eventId}")]
            public async Task<ActionResult<IEnumerable<EventAttendanceDTO>>> GetByEventId(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} получает посещения события {eventId}");

                return Ok(await _attendanceService.GetByEventIdAsync(eventId));
            }

            [HttpGet("user/{userId}/event/{eventId}")]
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

            [HttpGet("count/{eventId}")]
            public async Task<ActionResult<int>> CountParticipants(int eventId)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} считает участников события {eventId}");

                return Ok(await _attendanceService.CountParticipantsAsync(eventId));
            }

            [HttpPost]
            public async Task<ActionResult> Create(CreateEventAttendanceDTO dto)
            {
                var currUser = User.Identity?.IsAuthenticated == true
                    ? User.Identity.Name
                    : "Неавторизованный пользователь";

                _logger.LogInformation($"{currUser} создает посещение");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var created = await _attendanceService.AddAsync(dto);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }

            [HttpPut("{id}")]
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

            [HttpDelete("{id}")]
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

            [HttpDelete("soft/{id}")]
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
