using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class CreateReportDTO
    {
        public string SenderUserId { get; set; }
        public string ReportedUserId { get; set; }
        public string ReportReason { get; set; }
        public int ReportStatusId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
