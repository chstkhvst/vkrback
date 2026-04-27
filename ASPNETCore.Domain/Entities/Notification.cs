using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASPNETCore.Domain.Entities
{
    public class Notification
    {
        [Key] public int Id { get; set; }
        public string RecipientId { get; set; } = null!; 
        public User Recipient { get; set; } = null!;
        //public string? SenderId { get; set; } 
        //public User? Sender { get; set; }
        public string Message { get; set; } = null!;
        public int TypeId { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? EventId { get; set; }
        public VolunteerEvent? VolunteerEvent { get; set; }
    }
}
