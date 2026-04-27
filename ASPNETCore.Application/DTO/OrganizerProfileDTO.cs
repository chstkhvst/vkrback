using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class OrganizerProfileDTO
    {
        public OrganizerProfileDTO() { }
        public OrganizerProfileDTO(OrganizerProfile entity)
        {
            if (entity == null) return;
            UserId = entity.UserId;
            OrganizationName = entity.OrganizationName;
            Ogrn = entity.Ogrn;
            IsApproved = entity.IsApproved;
            ModeratedByUserId = entity.ModeratedByUserId;
        }
        public string UserId { get; set; }
        public string OrganizationName { get; set; }
        public string Ogrn { get; set; }
        public bool IsApproved { get; set; }
        public string? ModeratedByUserId { get; set; }
    }
}
