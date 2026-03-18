using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class CreateBanDTO
    {
        public string BannedUserId { get; set; }
        public string ModerId { get; set; }
        public bool IsActive { get; set; }= true;
        public bool IsDeleted { get; set; } = false;
    }
}
