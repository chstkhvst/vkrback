using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ASPNETCore.Application.Services
{
    public class ReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserReportDTO>> GetAllAsync()
        {
            var reports = await _reportRepository.GetAllAsync();
            return reports.Select(r => new UserReportDTO(r));
        }

        public async Task<UserReportDTO?> GetByIdAsync(int id)
        {
            var report = await _reportRepository.GetByIdAsync(id);

            if (report == null)
            {
                _logger.LogWarning($"Report with id {id} not found.");
                return null;
            }

            return new UserReportDTO(report);
        }

        public async Task<IEnumerable<UserReportDTO>> GetBySenderIdAsync(string senderId)
        {
            var reports = await _reportRepository.GetBySenderIdAsync(senderId);

            return reports
                .Where(r => r != null)
                .Select(r => new UserReportDTO(r));
        }

        public async Task<IEnumerable<UserReportDTO>> GetByReportedIdAsync(string reportedId)
        {
            var reports = await _reportRepository.GetByReportedIdAsync(reportedId);

            return reports
                .Where(r => r != null)
                .Select(r => new UserReportDTO(r));
        }

        public async Task<IEnumerable<UserReportDTO>> GetByStatusIdAsync(int statusId)
        {
            var reports = await _reportRepository.GetByStatusIdAsync(statusId);

            return reports
                .Where(r => r != null)
                .Select(r => new UserReportDTO(r));
        }

        public async Task<IEnumerable<UserReportDTO>> GetBySenderAndReportedAsync(string senderId, string reportedId)
        {
            var reports = await _reportRepository.GetBySenderAndReportedAsync(senderId, reportedId);

            return reports
                .Where(r => r != null)
                .Select(r => new UserReportDTO(r));
        }

        public async Task<UserReportDTO> AddAsync(CreateReportDTO dto)
        {
            var entity = new UserReport
            {
                SenderUserId = dto.SenderUserId,
                ReportedUserId = dto.ReportedUserId,
                ReportReason = dto.ReportReason,
                ReportStatusId = dto.ReportStatusId,
                IsDeleted = dto.IsDeleted
            };

            var created = await _reportRepository.AddAsync(entity);
            return new UserReportDTO(created);
        }

        public async Task<UserReportDTO> UpdateAsync(UserReportDTO dto)
        {
            var entity = new UserReport
            {
                Id = dto.Id,
                SenderUserId = dto.SenderUserId,
                ReportedUserId = dto.ReportedUserId,
                ReportReason = dto.ReportReason,
                ReportStatusId = dto.ReportStatusId,
                IsDeleted = dto.IsDeleted
            };

            try
            {
                var updated = await _reportRepository.UpdateAsync(entity);
                return new UserReportDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating report {dto.Id}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _reportRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting report {id}");
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                await _reportRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting report {id}");
                throw;
            }
        }
    }
}
