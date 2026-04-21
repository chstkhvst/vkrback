using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class UserForModerDTO
    {
        public UserForModerDTO() { }
        public UserForModerDTO(User entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            UserName = entity.UserName;
            VolunteerProfile = entity.VolunteerProfile != null ? new VolunteerProfileDTO(entity.VolunteerProfile) : null;
            OrganizerProfile = entity.OrganizerProfile != null ? new OrganizerProfileDTO(entity.OrganizerProfile) : null;
            ProfileImagePath = entity.ProfileImagePath;
            BackgroundImagePath = entity.BackgroundImagePath;
            Fullname = entity.FullName;
            Email = entity.Email;
            Bans = entity.Bans != null ? entity.Bans.Select(b => new BanDTO(b)).ToList() : new List<BanDTO>();
            UserReports = entity.UserReports != null ? entity.UserReports.Select(r => new UserReportDTO(r)).ToList() : new List<UserReportDTO>();

        }
        public string Id { get; set; }
        public string? UserName { get; set; }
        public string? Fullname { get; set; }
        public string Email { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? BackgroundImagePath { get; set; }
        public VolunteerProfileDTO? VolunteerProfile { get; set; }
        public OrganizerProfileDTO? OrganizerProfile { get; set; }
        public ICollection<BanDTO> Bans { get; set; }
        public ICollection<UserReportDTO> UserReports { get; set; }

    }
}
