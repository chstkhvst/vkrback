using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Entities
{
    public class UserReport
    {
        [Key] public int Id { get; set; }
        public string SenderUserId { get; set; }
        public string ReportedUserId { get; set; }
        public string ReportReason { get; set; }
        public string? ModeratedByUserId { get; set; }
        public int ReportStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public virtual User Sender { get; set; }
        public virtual User Reported { get; set; }
        public virtual User? Moder { get; set; }
        public virtual ReportStatus ReportStatus { get; set; }
    }
}
