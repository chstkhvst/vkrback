using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class CreateVolunteerEventDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string? Address { get; set; }
        public DateTime? EventDateTime { get; set; }
        public int EventPoints { get; set; }
        public int ParticipantsLimit { get; set; }
        //public string? ImagePath { get; set; }
        public IFormFile? Image { get; set; }
        public int EventCategoryId { get; set; }
        //public int EventStatusId { get; set; }
        public int CityId { get; set; }
        //public string? UserId { get; set; }
        //public string? ModeratedByUserId { get; set; }
        //public bool? IsDeleted { get; set; }
    }
}
