using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNETCore.Domain.Entities
{
    public class OrganizerProfile
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string OrganizationName { get; set; }
        public string Ogrn { get; set; }
    }
}
