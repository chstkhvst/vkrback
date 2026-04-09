using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class BanDTO
    {
        public BanDTO() { }
        public BanDTO(Ban entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            BannedUserId = entity.BannedUserId;
            ModerId = entity.ModerId;
            IsActive = entity.IsActive;
            IsDeleted = entity.IsDeleted;
            Moder = entity.Moder != null ? new UserDTO(entity.Moder) : null;
            BannedUser = entity.BannedUser != null ? new UserDTO(entity.BannedUser) : null;
        }
        public int Id { get; set; }
        public string BannedUserId { get; set; }
        public string BanReason { get; set; }
        public string ModerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public UserDTO? Moder { get; set; }
        public UserDTO? BannedUser { get; set; }
    }
}
