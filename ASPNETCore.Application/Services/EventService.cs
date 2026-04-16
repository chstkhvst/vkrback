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
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
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
                    e.EventStatusId != 5)
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

        public async Task<VolunteerEventDTO> UpdateAsync(VolunteerEventDTO dto)
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
                _logger.LogError(ex, $"Error updating event {dto.Id}");
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