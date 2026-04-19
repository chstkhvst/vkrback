using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class VolunteerProfileDTO
    {
        public VolunteerProfileDTO() { }
        public VolunteerProfileDTO(VolunteerProfile entity)
        {
            if (entity == null) return;

            UserId = entity.UserId;
            TotalPoints = entity.Points;
            Rank = entity.Rank != null ? new VolunteerRankDTO(entity.Rank) : null;
        }
        public string UserId { get; set; }
        public int TotalPoints { get; set; }
        public VolunteerRankDTO? Rank { get; set; }
    }
}
