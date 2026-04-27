using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ASPNETCore.Application.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly AttendanceService _attendanceService;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger,
            AttendanceService attendanceService)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _attendanceService = attendanceService;
        }

        public async Task<IEnumerable<NotificationDTO>> GetByRecipientIdAsync(string recipientId)
        {
            var notifications = await _notificationRepository.GetByRecipientIdAsync(recipientId);

            return notifications
                .Select(n => new NotificationDTO(n));
        }

        public async Task<IEnumerable<NotificationDTO>> GetUnreadByRecipientIdAsync(string recipientId)
        {
            var notifications = await _notificationRepository.GetUnreadByRecipientIdAsync(recipientId);

            return notifications
                .Select(n => new NotificationDTO(n));
        }
        public async Task<int> GetUnreadCountAsync(string recipientId)
        {
            return await _notificationRepository.GetUnreadCountAsync(recipientId);
        }

        public async Task<NotificationDTO> AddAsync(CreateNotificationDTO dto)
        {
            var entity = new Notification
            {
                RecipientId = dto.RecipientId,
                Message = dto.Message,
                TypeId = dto.TypeId,
                EventId = dto.EventId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.ToLocalTime(),
            };

            var created = await _notificationRepository.AddAsync(entity);
            return new NotificationDTO(created);
        }

        public async Task<IEnumerable<NotificationDTO>> AddRangeAsync(CreateNotificationDTO dto)
        {
            var attendees = await _attendanceService.GetByEventIdAsync(dto.EventId);
            
            var notifications = attendees
                .Where(a => a != null && !a.IsDeleted)
                .Select(a => new Notification
                {
                    RecipientId = a.UserId,
                    Message = dto.Message,
                    TypeId = dto.TypeId,
                    EventId = dto.EventId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (!notifications.Any())
                return Enumerable.Empty<NotificationDTO>();

            var created = await _notificationRepository.AddRangeAsync(notifications);

            return created.Select(n => new NotificationDTO(n));
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notificationId} as read");
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(string recipientId)
        {
            try
            {
                await _notificationRepository.MarkAllAsReadAsync(recipientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking all notifications as read for user {recipientId}");
                throw;
            }
        }
    }
}