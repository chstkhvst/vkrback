using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class Ban
    {
        [Key] public int Id { get; set; }
        public string BannedUserId { get; set; } = null!;
        public string ModerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public virtual User Moder { get; set; }
        public virtual User BannedUser { get; set; }
    }
}
