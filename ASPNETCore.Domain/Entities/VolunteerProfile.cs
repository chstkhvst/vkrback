using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Entities
{
    public class VolunteerProfile
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int RankId { get; set; }
        public int Points { get; set; }
        public int Coins  { get; set; }
        public virtual VolunteerRank Rank { get; set; }
    }
}
