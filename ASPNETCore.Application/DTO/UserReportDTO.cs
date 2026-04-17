using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class UserReportDTO
    {
        public UserReportDTO() { }
        public UserReportDTO(UserReport entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            SenderUserId = entity.SenderUserId;
            ReportedUserId = entity.ReportedUserId;
            ReportReason = entity.ReportReason;
            ReportStatusId = entity.ReportStatusId;
            ModeratedByUserId = entity.ModeratedByUserId;
            IsDeleted = entity.IsDeleted;
            Sender = entity.Sender != null ? new UserDTO(entity.Sender) : null;
            Reported = entity.Reported != null ? new UserDTO(entity.Reported) : null;
            if (entity.ReportStatus != null)
            {
                ReportStatus = new ReportStatusDTO
                {
                    Id = entity.ReportStatus.Id,
                    Name = entity.ReportStatus.ReportStatusName
                };
            }
        }
        public int Id { get; set; }
        public string SenderUserId { get; set; }
        public string ReportedUserId { get; set; }
        public string ReportReason { get; set; }
        public string ModeratedByUserId { get; set; }
        public int ReportStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public UserDTO? Sender { get; set; }
        public UserDTO? Reported { get; set; } 
        public ReportStatusDTO? ReportStatus { get; set; }
    }
}
