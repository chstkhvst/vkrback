using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ASPNETCore.Application.Services
{
    public class AttendanceService
    {
        private readonly IAttendanceRepositiory _attendanceRepository;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            IAttendanceRepositiory attendanceRepository,
            ILogger<AttendanceService> logger)
        {
            _attendanceRepository = attendanceRepository;
            _logger = logger;
        }

        public async Task<EventAttendanceDTO?> GetByIdAsync(int id)
        {
            var entity = await _attendanceRepository.GetByIdAsync(id);
            return entity == null ? null : new EventAttendanceDTO(entity);
        }

        public async Task<IEnumerable<EventAttendanceDTO>> GetAllAsync()
        {
            var entities = await _attendanceRepository.GetAllAsync();
            return entities.Select(a => new EventAttendanceDTO(a));
        }

        public async Task<IEnumerable<EventAttendanceDTO>> GetByUserIdAsync(string userId)
        {
            var entities = await _attendanceRepository.GetByUserIdAsync(userId);

            return entities
                .Select(a => new EventAttendanceDTO(a!));
        }

        public async Task<IEnumerable<EventAttendanceDTO>> GetByEventIdAsync(int eventId)
        {
            var entities = await _attendanceRepository.GetByEventIdAsync(eventId);

            return entities
                .Select(a => new EventAttendanceDTO(a!));
        }

        public async Task<EventAttendanceDTO?> GetByUserAndEventAsync(string userId, int eventId)
        {
            var entity = await _attendanceRepository.GetByUserAndEventAsync(userId, eventId);
            return entity == null ? null : new EventAttendanceDTO(entity);
        }

        public async Task<int> CountParticipantsAsync(int eventId)
        {
            return await _attendanceRepository.CountParticipantsAsync(eventId);
        }

        public async Task<EventAttendanceDTO> AddAsync(CreateEventAttendanceDTO dto)
        {
            var entity = new EventAttendance
            {
                EventId = dto.EventId,
                UserId = dto.UserId,
                AttendanceStatusId = dto.AttendanceStatusId,
                IsDeleted = false
            };

            var created = await _attendanceRepository.AddAsync(entity);

            return new EventAttendanceDTO(created);
        }

        public async Task<EventAttendanceDTO> UpdateAsync(EventAttendanceDTO dto)
        {
            var entity = new EventAttendance
            {
                Id = dto.Id,
                EventId = dto.EventId,
                UserId = dto.UserId,
                AttendanceStatusId = dto.AttendanceStatusId,
                IsDeleted = dto.IsDeleted
            };

            try
            {
                var updated = await _attendanceRepository.UpdateAsync(entity);
                return new EventAttendanceDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating attendance {dto.Id}");
                throw;
            }
        }
        public async Task MarkNoShowAsync(int eventId)
        {
            try
            {
                var attendances = await _attendanceRepository.GetByEventIdAsync(eventId);
                var upcomingAttendances = attendances
                    .Where(a => a.AttendanceStatusId == 1 && !a.IsDeleted) //ожидается
                    .ToList();

                if (!upcomingAttendances.Any())
                    return;

                foreach (var attendance in upcomingAttendances)
                {
                    attendance.AttendanceStatusId = 4; //неявка
                    await _attendanceRepository.UpdateAsync(attendance);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отметке неявок для мероприятия {eventId}");
                throw;
            }
        }
        public async Task DeleteAsync(int id)
        {
            try
            {
                await _attendanceRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attendance {id}");
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                await _attendanceRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting attendance {id}");
                throw;
            }
        }
    }
}