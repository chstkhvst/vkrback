using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Model;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly ILogger<EventController> _logger;
        private readonly IWebHostEnvironment _env;

        public EventController(EventService eventService, ILogger<EventController> logger, IWebHostEnvironment env)
        {
            _env = env;
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<VolunteerEventDTO>>> GetAllEvents(
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
        [HttpGet("[action]")]
        public async Task<ActionResult<PaginatedResponse<VolunteerEventDTO>>> GetPagedEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? catId = null,
            [FromQuery] int? cityId = null,
            [FromQuery] string? keyWords = null,
            [FromQuery] DateTime? dateTime = null)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает страницу событий");

            var result = await _eventService.GetPagedAsync(
                pageNumber,
                pageSize,
                catId,
                cityId,
                keyWords,
                dateTime
            );

            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<PaginatedResponse<VolunteerEventDTO>>> GetPagedForUser(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? catId = null,
            [FromQuery] int? cityId = null,
            [FromQuery] string? keyWords = null,
            [FromQuery] DateTime? dateTime = null)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает страницу событий (для пользователя)");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                userId = "";

            var result = await _eventService.GetPagedForUserAsync(
                userId,
                pageNumber,
                pageSize,
                catId,
                cityId,
                keyWords,
                dateTime
            );

            return Ok(result);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<VolunteerEventDTO>> GetEventById(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает событие {id}");

            var ev = await _eventService.GetByIdAsync(id);

            if (ev == null)
                return NotFound();

            return Ok(ev);
        } 
        [HttpGet("[action]/{userId}")]
        public async Task<ActionResult<IEnumerable<VolunteerEventDTO>>> GetEventsByUserId(string userId)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает все созданные ивенты от {userId}");

            var ev = await _eventService.GetEventsByUserIdAsync(userId);

            if (ev == null)
                return NotFound();

            return Ok(ev);
        }
        [HttpGet("geocode")]
        public async Task<ActionResult<List<GeocodeResult>>> Geocode(string query)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={query}";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("vkr-localhost-volunteer");

            var result = await httpClient.GetFromJsonAsync<List<GeocodeResult>>(url);

            return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<ActionResult<VolunteerEventDTO>> CreateEvent(
            [FromForm] CreateVolunteerEventDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} создает событие");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //ID текущего пользователя
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Пользователь не авторизован");

            var created = await _eventService.AddAsync(dto,userId, _env);

            return Ok(created);
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult> UpdateEvent(int id, VolunteerEventDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} обновляет событие {id}");
            var isModerator = User.IsInRole("moderator");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (isModerator)
            {
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    dto.ModeratedByUserId = currentUserId;
                }
            }
            else if (currentUserId != dto.UserId)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest();

            var updated = await _eventService.UpdateAsync(dto);

            return Ok(updated);
        }

        [HttpDelete("[action]/{id}")]
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

        [HttpDelete("[action]/{id}")]
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