using System.ComponentModel.DataAnnotations;

namespace ASPNETCore.Domain.Entities
{
    public class EventAttendance
    {
        [Key] public int Id { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; }
        public int AttendanceStatusId { get; set; }
        public virtual VolunteerEvent VolunteerEvent { get; set; }
        public virtual AttendanceStatus AttendanceStatus { get; set; }
        public virtual User User { get; set; }
        public bool IsDeleted { get; set; }
    }
}
