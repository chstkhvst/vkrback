using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByRecipientIdAsync(string recipientId)
        {
            return await _context.Notifications
                .Include(n => n.NotificationType)
                .Include(n => n.VolunteerEvent)
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByRecipientIdAsync(string recipientId)
        {
            return await _context.Notifications
                .Include(n => n.NotificationType)
                .Include(n => n.VolunteerEvent)
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<int> GetUnreadCountAsync(string recipientId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> AddRangeAsync(IEnumerable<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
            return notifications;
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string recipientId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .ToListAsync();

            if (notifications.Any())
            {
                foreach (var n in notifications)
                {
                    n.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}