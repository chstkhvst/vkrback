using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class EventStatus
    {
        // checked
        [Key] public int Id { get; set; }
        [Required] public string EventStatusName { get; set; }
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public virtual ICollection<VolunteerEvent> VEvents { get; set; }
    }
}
