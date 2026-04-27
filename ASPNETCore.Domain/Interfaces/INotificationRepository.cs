using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Interfaces
{
    public interface INotificationRepository
    {
        // все уведы пользователя
        Task<IEnumerable<Notification>> GetByRecipientIdAsync(string recipientId);

        // только непрочитанные 
        Task<IEnumerable<Notification>> GetUnreadByRecipientIdAsync(string recipientId);
        Task<int> GetUnreadCountAsync(string recipientId);
        Task<Notification> AddAsync(Notification notification);

        // массовое создание
        Task<IEnumerable<Notification>> AddRangeAsync(IEnumerable<Notification> notifications);

        // отметить как прочитанное
        Task MarkAsReadAsync(int notificationId);

        // отметить все
        Task MarkAllAsReadAsync(string recipientId);
    }
}
