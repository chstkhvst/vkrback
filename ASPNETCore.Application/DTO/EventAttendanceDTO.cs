using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class EventAttendanceDTO
    {
        public EventAttendanceDTO() { }
        public EventAttendanceDTO(EventAttendance entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            EventId = entity.EventId;
            UserId = entity.UserId;
            AttendanceStatusId = entity.AttendanceStatusId;
            IsDeleted = entity.IsDeleted;
            VolunteerEvent = entity.VolunteerEvent != null ? new VolunteerEventDTO(entity.VolunteerEvent) : null;
            if (entity.AttendanceStatus != null)
            {
                AttendanceStatus = new AttendanceStatusDTO
                {
                    Id = entity.AttendanceStatus.Id,
                    Name = entity.AttendanceStatus.AttendanceStatusName
                };
            }
            User = entity.User != null ? new UserDTO(entity.User) : null;
        }
        public int Id { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; }
        public int AttendanceStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public VolunteerEventDTO? VolunteerEvent { get; set; }
        public AttendanceStatusDTO? AttendanceStatus { get; set; }
        public UserDTO? User { get; set; }
    }
}
