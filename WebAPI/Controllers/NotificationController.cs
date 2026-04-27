using Microsoft.AspNetCore.Mvc;
using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using System.Security.Claims;

namespace ASPNETCore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            NotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("[action]/{recipientId}")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetByRecipientId(string recipientId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} получает уведомления пользователя {recipientId}");

            return Ok(await _notificationService.GetByRecipientIdAsync(recipientId));
        }

        [HttpGet("[action]/{recipientId}")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUnreadByRecipientId(string recipientId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != recipientId)
            {
                _logger.LogWarning($"Пользователь {currUser} пытается получить количество уведомлений пользователя {recipientId}");
                return Unauthorized(new { message = "Нет доступа к данным другого пользователя" });
            }
            _logger.LogInformation($"{currUser} получает непрочитанные уведомления пользователя {recipientId}");

            return Ok(await _notificationService.GetUnreadByRecipientIdAsync(recipientId));
        }
        [HttpGet("unread-count/{recipientId}")]
        public async Task<ActionResult<int>> GetUnreadCount(string recipientId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != recipientId)
            {
                _logger.LogWarning($"Пользователь {currUser} пытается получить количество уведомлений пользователя {recipientId}");
                return Unauthorized(new { message = "Нет доступа к данным другого пользователя" });
            }
            _logger.LogInformation($"{currUser} получает количество непрочитанных уведомлений пользователя {recipientId}");

            var count = await _notificationService.GetUnreadCountAsync(recipientId);

            return Ok(count);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> CreateNotification(CreateNotificationDTO dto)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} создает уведомление");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _notificationService.AddAsync(dto);

            return Ok(created);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> CreateForEvent(CreateNotificationDTO dto) 
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} создает уведомления для события {dto.EventId}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _notificationService.AddRangeAsync(dto);

            return Ok(created);
        }

        [HttpPut("[action]/{notificationId}")]
        public async Task<ActionResult> MarkAsRead(int notificationId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} отмечает уведомление {notificationId} как прочитанное");

            try
            {
                await _notificationService.MarkAsReadAsync(notificationId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отметке уведомления {notificationId}");
                return StatusCode(500);
            }
        }

        [HttpPut("[action]/{recipientId}")]
        public async Task<ActionResult> MarkAllAsRead(string recipientId)
        {
            var currUser = User.Identity?.IsAuthenticated == true
                ? User.Identity.Name
                : "Неавторизованный пользователь";

            _logger.LogInformation($"{currUser} отмечает все уведомления пользователя {recipientId}");

            try
            {
                await _notificationService.MarkAllAsReadAsync(recipientId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отметке всех уведомлений пользователя {recipientId}");
                return StatusCode(500);
            }
        }
    }
}