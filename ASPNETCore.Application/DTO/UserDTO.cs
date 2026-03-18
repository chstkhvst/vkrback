using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class UserDTO
    {
        public UserDTO() { }
        public UserDTO(User entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            UserName = entity.UserName;
            VolunteerProfile = entity.VolunteerProfile != null ? new VolunteerProfileDTO(entity.VolunteerProfile) : null;
            OrganizerProfile = entity.OrganizerProfile != null ? new OrganizerProfileDTO(entity.OrganizerProfile) : null;
        }
        public string Id { get; set; }
        public string? UserName { get; set; }
        public VolunteerProfileDTO? VolunteerProfile { get; set; }
        public OrganizerProfileDTO? OrganizerProfile { get; set; }
    }
}
