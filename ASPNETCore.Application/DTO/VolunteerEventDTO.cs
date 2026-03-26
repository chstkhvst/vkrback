using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ASPNETCore.Application.DTO
{
    public class VolunteerEventDTO
    {
        public VolunteerEventDTO() { }
        public VolunteerEventDTO(VolunteerEvent entity)
        {
            if (entity == null) return;

            Id = entity.Id;
            Name = entity.Name;
            Description = entity.Description;
            Lat = entity.Lat.HasValue ? (double?)entity.Lat.Value : null;
            Lng = entity.Lng.HasValue ? (double?)entity.Lng.Value : null;
            Address = entity.Address;
            EventDateTime = entity.EventDateTime;
            EventPoints = entity.EventPoints;
            ParticipantsLimit = entity.ParticipantsLimit;
            ImagePath = entity.ImagePath;
            EventCategoryId = entity.EventCategoryId;
            EventStatusId = entity.EventStatusId;
            CityId = entity.CityId;
            UserId = entity.UserId;
            ModeratedByUserId = entity.ModeratedByUserId;
            IsDeleted = entity.IsDeleted;

            if (entity.EventCategory != null)
            {
                EventCategory = new EventCategoryDTO
                {
                    Id = entity.EventCategory.Id,
                    Name = entity.EventCategory.CategoryName
                };
            }

            if (entity.EventStatus != null)
            {
                EventStatus = new EventStatusDTO
                {
                    Id = entity.EventStatus.Id,
                    Name = entity.EventStatus.EventStatusName
                };
            }

            if (entity.City != null)
            {
                City = new CityDTO
                {
                    Id = entity.City.Id,
                    Name = entity.City.Name,
                    Subject = entity.City.Subject
                };
            }
            User = entity.User != null ? new UserDTO(entity.User) : null;
            ModeratedByUser = entity.ModeratedByUser != null ? new UserDTO(entity.ModeratedByUser) : null;

            //Attendees = entity.Attendees != null && entity.Attendees.Any()
            //    ? entity.Attendees.Select(a => new EventAttendanceDTO(a)).ToList()
            //    : new List<EventAttendanceDTO>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string? Address { get; set; }
        public DateTime? EventDateTime { get; set; }
        public int EventPoints { get; set; }
        public int ParticipantsLimit { get; set; }
        public string? ImagePath { get; set; }
        public int EventCategoryId { get; set; }
        public int EventStatusId { get; set; }
        public int CityId { get; set; }
        public string UserId { get; set; }
        public string? ModeratedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public EventCategoryDTO? EventCategory { get; set; }
        public EventStatusDTO? EventStatus { get; set; }
        public CityDTO? City { get; set; }
        public UserDTO? User { get; set; }
        public UserDTO? ModeratedByUser { get; set; }
        public ICollection<EventAttendanceDTO>? Attendees { get; set; }
    }
}
