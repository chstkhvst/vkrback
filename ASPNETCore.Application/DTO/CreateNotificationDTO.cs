using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class CreateNotificationDTO
    {
        public string? RecipientId { get; set; }
        public string Message { get; set; }
        public int TypeId { get; set; }
        public int EventId { get; set; }
    }
}
