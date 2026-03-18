using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    // checked
    public class AttendanceStatus
    {
        [Key] public int Id { get; set; }
        [Required] public string AttendanceStatusName { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<EventAttendance> Attendance { get; set; }
    }
}
