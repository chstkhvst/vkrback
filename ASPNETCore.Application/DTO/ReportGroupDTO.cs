using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class ReportGroupDTO
    {
        public string ReportedUserId { get; set; }
        public UserDTO ReportedUser { get; set; }
        public int Count { get; set; }
        public List<UserReportDTO> Reports { get; set; }
    }
}
