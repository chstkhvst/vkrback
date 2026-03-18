using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class CreateEventAttendanceDTO
    {
        public int EventId { get; set; }
        public string UserId { get; set; }
        public int AttendanceStatusId { get; set; }
    }
}
