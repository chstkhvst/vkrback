using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(EventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolunteerEventDTO>>> GetEvents(
            [FromQuery] int? catId,
            [FromQuery] int? cityId,
            [FromQuery] string? keyWords,
            [FromQuery] DateTime? dateTime)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает список событий");

            if (catId == null && cityId == null && keyWords == null && dateTime == null)
                return Ok(await _eventService.GetAllAsync());

            return Ok(await _eventService.GetFilteredAsync(catId, cityId, keyWords, dateTime));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VolunteerEventDTO>> GetEventById(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает событие {id}");

            var ev = await _eventService.GetByIdAsync(id);

            if (ev == null)
                return NotFound();

            return Ok(ev);
        }

        [HttpPost]
        public async Task<ActionResult<VolunteerEventDTO>> CreateEvent(CreateVolunteerEventDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} создает событие");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _eventService.AddAsync(dto);

            return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEvent(int id, VolunteerEventDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} обновляет событие {id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest();

            var updated = await _eventService.UpdateAsync(dto);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} удаляет событие {id}");

            try
            {
                await _eventService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления события {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("soft/{id}")]
        public async Task<IActionResult> SoftDeleteEvent(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} мягко удаляет событие {id}");

            try
            {
                await _eventService.SoftDeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка soft delete события {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }
    }
}