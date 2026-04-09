using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BanController : ControllerBase
    {
        private readonly BanService _banService;
        private readonly ILogger<BanController> _logger;

        public BanController(BanService banService, ILogger<BanController> logger)
        {
            _banService = banService;
            _logger = logger;
        }

        [HttpGet("[action]/{userId}")]
        public async Task<ActionResult<bool>> IsUserBanned(string userId)
        {
            try
            {
                var isBanned = await _banService.IsUserBannedAsync(userId);
                return Ok(isBanned);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при проверке бана пользователя {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Ошибка при проверке статуса бана" });
            }
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<BanDTO>>> GetBans()
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает список банов");

            var bans = await _banService.GetAllAsync();
            return Ok(bans);
        }
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<BanDTO>> GetBanById(int id)
        {
            var ban = await _banService.GetByIdAsync(id);

            if (ban == null)
                return NotFound();

            return Ok(ban);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<BanDTO>> CreateBan(CreateBanDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                dto.ModerId = currentUserId;
            }
            _logger.LogInformation($"{currUser} создает бан");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _banService.AddAsync(dto);

            return Ok(created);
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult> UpdateBan(int id, BanDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} обновляет бан {id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest();

            var updated = await _banService.UpdateAsync(dto);

            return Ok(updated);
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteBan(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} удаляет бан {id}");

            try
            {
                await _banService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления бана {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> SoftDeleteBan(int id)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} удаляет бан {id}");

            try
            {
                await _banService.SoftDeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка soft delete бана {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
        }
    }
}