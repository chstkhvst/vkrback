using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? BackgroundImagePath { get; set; }
        public virtual VolunteerProfile? VolunteerProfile { get; set; }
        public virtual OrganizerProfile? OrganizerProfile { get; set; }
        public bool IsDeleted { get; set; }

        // мероприятия, которые пользователь организовал
        [JsonIgnore]
        public ICollection<VolunteerEvent> OrganizedEvents { get; set; }

        // регистрации пользователя на мероприятия
        [JsonIgnore]
        public ICollection<EventAttendance> EventAttendance { get; set; }

        // баны
        [JsonIgnore]
        public ICollection<Ban> Bans { get; set; }

        // жалобы
        [JsonIgnore]
        public ICollection<UserReport> UserReports { get; set; }

        [JsonIgnore]
        public ICollection<Notification> Notifications { get; set; }
    }
}