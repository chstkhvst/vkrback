using System.ComponentModel.DataAnnotations;

namespace ASPNETCore.Domain.Entities
{
    // checked
    public class City
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string? Subject { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<VolunteerEvent> VEvents { get; set; }
    }
}
