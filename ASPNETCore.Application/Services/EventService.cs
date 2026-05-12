using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ASPNETCore.Application.Services
{
    public class EventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IAttendanceRepositiory _attRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, IAttendanceRepositiory attRepository, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _attRepository = attRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<VolunteerEventDTO>> GetAllAsync()
        {
            var events = await _eventRepository.GetAllAsync();
            return events.Select(e => new VolunteerEventDTO(e));
        }

        public async Task<VolunteerEventDTO?> GetByIdAsync(int id)
        {
            var ev = await _eventRepository.GetByIdAsync(id);

            if (ev == null)
            {
                _logger.LogWarning($"Event with id {id} not found.");
                return null;
            }

            return new VolunteerEventDTO(ev);
        }

        public async Task<IEnumerable<VolunteerEventDTO>> GetFilteredAsync(
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var events = await _eventRepository.GetFilteredAsync(catId, cityId, keyWords, dateTime);
            return events.Select(e => new VolunteerEventDTO(e));
        }
        // все ивенты созданные организаторами для просмотра модерами
        public async Task<PaginatedResponse<VolunteerEventDTO>> GetPagedOrgAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime, 
            int? statusId)
        {
            var result = await _eventRepository.GetPagedOrgAsync(pageNumber, pageSize, catId, cityId, keyWords, dateTime, statusId);

            return new PaginatedResponse<VolunteerEventDTO>
            {
                Items = result.Items.Select(e => new VolunteerEventDTO(e)).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages
            };
        }
        // для получения модером всех ивентов созданных волонтерами
        public async Task<PaginatedResponse<VolunteerEventDTO>> GetPagedCommunityEventsAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime,
            int? statusId)
        {
            var result = await _eventRepository.GetPagedCommunityEventsAsync(pageNumber, pageSize, catId, cityId, keyWords, dateTime, statusId);

            return new PaginatedResponse<VolunteerEventDTO>
            {
                Items = result.Items.Select(e => new VolunteerEventDTO(e)).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages
            };
        }
        private async Task UpdateCompletedEventsAsync()
        {
            var now = DateTime.UtcNow.ToLocalTime();

            var eventsToUpdate = await _eventRepository.GetAllAsync();

            foreach (var e in eventsToUpdate)
            {
                if (e.EventDateTime.HasValue &&
                    e.EventDateTime.Value < now &&
                    e.EventStatusId != 5 && e.EventStatusId != 3 && e.EventStatusId != 4)
                {
                    e.EventStatusId = 5;
                    await _eventRepository.UpdateAsync(e);
                }
            }
        }
        // все ивенты созданные огранизаторами для просмотра в роли волонтера и организатора
        public async Task<PaginatedResponse<VolunteerEventDTO>> GetPagedOrgForUserAsync(
            string userId,
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            await UpdateCompletedEventsAsync();

            var result = await _eventRepository.GetPagedOrgForUserAsync(
                userId,
                pageNumber,
                pageSize,
                catId,
                cityId,
                keyWords,
                dateTime
            );

            return new PaginatedResponse<VolunteerEventDTO>
            {
                Items = result.Items.Select(e => new VolunteerEventDTO(e)).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages
            };
        }
        // созданные волонтерами ивенты для отображения юзерам 
        public async Task<PaginatedResponse<VolunteerEventDTO>> GetPagedCommunityEventsForUserAsync(
            string userId,
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            await UpdateCompletedEventsAsync();

            var result = await _eventRepository.GetPagedCommunityEventsForUserAsync(
                userId,
                pageNumber,
                pageSize,
                catId,
                cityId,
                keyWords,
                dateTime
            );

            return new PaginatedResponse<VolunteerEventDTO>
            {
                Items = result.Items.Select(e => new VolunteerEventDTO(e)).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages
            };
        }
        public async Task<IEnumerable<VolunteerEventDTO>> GetEventsByUserIdAsync(string userId)
        {
            var entities = await _eventRepository.GetEventsByUserIdAsync(userId);

            return entities
                .Select(a => new VolunteerEventDTO(a!));
        }
        public async Task<VolunteerEventDTO> AddAsync(CreateVolunteerEventDTO dto, string userId, IWebHostEnvironment env)
        {
            var entity = new VolunteerEvent
            {
                Name = dto.Name,
                Description = dto.Description,
                Lat = double.Parse(dto.Lat, CultureInfo.InvariantCulture),
                Lng = double.Parse(dto.Lng, CultureInfo.InvariantCulture),
                Address = dto.Address,
                EventDateTime = dto.EventDateTime?.ToLocalTime(),
                EventPoints = dto.EventPoints,
                ParticipantsLimit = dto.ParticipantsLimit,
                EventCategoryId = dto.EventCategoryId,
                EventStatusId = 1, // на модерации
                CityId = dto.CityId,
                UserId = userId,
                ModeratedByUserId = null,
                IsDeleted = false
            };

            // изображение
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsFolder = env.WebRootPath;

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                entity.ImagePath = $"/{uniqueFileName}";
            }

            var created = await _eventRepository.AddAsync(entity);

            return new VolunteerEventDTO(created);
        }

        //public async Task<VolunteerEventDTO> UpdateAsync(VolunteerEventDTO dto)
        //{
        //    var entity = new VolunteerEvent
        //    {
        //        Id = dto.Id,
        //        Name = dto.Name,
        //        Description = dto.Description,
        //        Lat = dto.Lat,
        //        Lng = dto.Lng,
        //        Address = dto.Address,
        //        EventDateTime = dto.EventDateTime?.ToLocalTime(),
        //        EventPoints = dto.EventPoints,
        //        ParticipantsLimit = dto.ParticipantsLimit,
        //        ImagePath = dto.ImagePath,
        //        EventCategoryId = dto.EventCategoryId,
        //        EventStatusId = dto.EventStatusId,
        //        CityId = dto.CityId,
        //        UserId = dto.UserId,
        //        ModeratedByUserId = dto.ModeratedByUserId,
        //        IsDeleted = dto.IsDeleted
        //    };

        //    try
        //    {
        //        var updated = await _eventRepository.UpdateAsync(entity);
        //        return new VolunteerEventDTO(updated);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error updating event {dto.Id}");
        //        throw;
        //    }
        //}
        public async Task<VolunteerEventDTO> UpdateByModeratorAsync(VolunteerEventDTO dto)
        {
            var entity = new VolunteerEvent
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Lat = dto.Lat,
                Lng = dto.Lng,
                Address = dto.Address,
                EventDateTime = dto.EventDateTime?.ToLocalTime(),
                EventPoints = dto.EventPoints,
                ParticipantsLimit = dto.ParticipantsLimit,
                ImagePath = dto.ImagePath,
                EventCategoryId = dto.EventCategoryId,
                EventStatusId = dto.EventStatusId,
                CityId = dto.CityId,
                UserId = dto.UserId,
                ModeratedByUserId = dto.ModeratedByUserId,
                IsDeleted = dto.IsDeleted
            };

            try
            {
                var updated = await _eventRepository.UpdateAsync(entity);
                return new VolunteerEventDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating event {dto.Id} by moderator");
                throw;
            }
        }
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadius = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }
        public async Task<VolunteerEventDTO> UpdateByOrganizerAsync(UpdateEventDTO dto, string userId)
        {
            var existing = await _eventRepository.GetByIdAsync(dto.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Event {dto.Id} not found");    

            if (existing.UserId != userId)
                throw new UnauthorizedAccessException();

            if (dto.EventStatusId.HasValue && dto.EventStatusId.Value != existing.EventStatusId)
                existing.EventStatusId = dto.EventStatusId.Value;

            if (dto.Description != null)
                existing.Description = dto.Description;

            if (dto.EventDateTime.HasValue)
                existing.EventDateTime = dto.EventDateTime.Value.ToLocalTime();

            //if (dto.Lat.HasValue)
            //    existing.Lat = dto.Lat;

            //if (dto.Lng.HasValue)
            //    existing.Lng = dto.Lng;
            if (dto.Lat.HasValue && dto.Lng.HasValue && existing.Lat.HasValue && existing.Lng.HasValue)
            {
                const double maxDistanceKm = 15;
                var distance = CalculateDistance(
                    existing.Lat.Value, existing.Lng.Value, dto.Lat.Value, dto.Lng.Value);

                if (distance > maxDistanceKm)
                {
                    throw new Exception(
                        $"Cannot move event" 
                    );
                }
                existing.Lat = dto.Lat.Value;
                existing.Lng = dto.Lng.Value;
            }

            if (dto.Address != null)
                existing.Address = dto.Address;

            if (dto.ParticipantsLimit.HasValue)
            {
                var currParticipants = await _attRepository.CountParticipantsAsync(dto.Id);
                if (currParticipants > dto.ParticipantsLimit.Value)
                    throw new Exception("Number of participants exceeds the limit");
                existing.ParticipantsLimit = dto.ParticipantsLimit.Value;
            }

            try
            {
                var updated = await _eventRepository.UpdateAsync(existing);
                return new VolunteerEventDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating event {dto.Id} by organizer");
                throw;
            }
        }
        public async Task DeleteAsync(int id)
        {
            try
            {
                await _eventRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event {id}");
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                await _eventRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting event {id}");
                throw;
            }
        }
    }
}