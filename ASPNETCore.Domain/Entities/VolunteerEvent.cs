using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class VolunteerEvent
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Address { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public DateTime? EventDateTime { get; set; }
        public int EventPoints { get; set; }
        public int ParticipantsLimit { get; set; }
        // ?
        public string? ImagePath { get; set; }
        public int EventCategoryId { get; set; }
        public int EventStatusId { get; set; }
        public int CityId { get; set; }
        public string UserId { get; set; }
        public string? ModeratedByUserId { get; set; }
        public virtual EventCategory EventCategory { get; set; }
        public virtual EventStatus EventStatus { get; set; }
        public virtual City City { get; set; }
        public virtual User User { get; set; }
        public virtual User? ModeratedByUser { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<EventAttendance> Attendees { get; set; }
    }
}
