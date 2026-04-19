using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Entities
{
    public class VolunteerRank
    {
        [Key] public int Id { get; set; }
        [Required] public string RankName { get; set; }
        public int PointsRequired { get; set; } //?
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<VolunteerProfile> Users { get; set; }
    }
}
