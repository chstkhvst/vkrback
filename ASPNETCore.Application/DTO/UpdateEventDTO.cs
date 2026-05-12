using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class UpdateEventDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDateTime { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string? Address { get; set; }
        public int? ParticipantsLimit { get; set; }
        public int? EventStatusId { get; set; }
    }
}
