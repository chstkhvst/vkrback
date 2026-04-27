using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class NotificationDTO
    {
        public NotificationDTO() { }

        public NotificationDTO(Notification entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            RecipientId = entity.RecipientId;
            Message = entity.Message;
            TypeId = entity.TypeId;
            IsRead = entity.IsRead;
            CreatedAt = entity.CreatedAt;
            EventId = entity.EventId;

            Recipient = entity.Recipient != null ? new UserDTO(entity.Recipient) : null;

            if (entity.NotificationType != null)
            {
                NotificationType = new NotificationTypeDTO
                {
                    Id = entity.NotificationType.Id,
                    Name = entity.NotificationType.NotificationTypeName
                };
            }

            //if (entity.VolunteerEvent != null)
            //{
            //    VolunteerEvent = new VolunteerEventDTO(entity.VolunteerEvent);
            //}
        }

        public int Id { get; set; }
        public string RecipientId { get; set; }
        public string Message { get; set; }
        public int TypeId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? EventId { get; set; }

        public UserDTO? Recipient { get; set; }
        public NotificationTypeDTO? NotificationType { get; set; }
        //public VolunteerEventDTO? VolunteerEvent { get; set; }
    }
}
