using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

        public async Task<PaginatedResponse<VolunteerEventDTO>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var result = await _eventRepository.GetPagedAsync(pageNumber, pageSize, catId, cityId, keyWords, dateTime);

            return new PaginatedResponse<VolunteerEventDTO>
            {
                Items = result.Items.Select(e => new VolunteerEventDTO(e)).ToList(),
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages
            };
        }

        public async Task<VolunteerEventDTO> AddAsync(CreateVolunteerEventDTO dto, string userId, IWebHostEnvironment env)
        {
            var entity = new VolunteerEvent
            {
                Name = dto.Name,
                Description = dto.Description,
                Lat = dto.Lat,
                Lng = dto.Lng,
                Address = dto.Address,
                EventDateTime = dto.EventDateTime,
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
                EventDateTime = dto.EventDateTime,
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