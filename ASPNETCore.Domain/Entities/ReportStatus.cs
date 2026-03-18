using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class ReportStatus
    {
        //checked
        [Key] public int Id { get; set; }
        [Required] public string ReportStatusName { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserReport> UserReports { get; set; }
    }
}
