using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace ASPNETCore.Application.Services
{
    public class ReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IBanRepository _banRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, IBanRepository banRepository, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _banRepository = banRepository;
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
        public async Task<IEnumerable<ReportGroupDTO>> GetGroupedReportsAsync(int? statusId = null, string? keywords = null)
        {
            var groups = await _reportRepository.GetGroupedReportsAsync(statusId, keywords);

            return groups.Select(g => new ReportGroupDTO
            {
                ReportedUserId = g.ReportedUserId,
                ReportedUser = g.ReportedUser != null ? new UserDTO(g.ReportedUser) : null,
                Count = g.Count,
                Reports = g.Reports
                    .Select(r => new UserReportDTO(r))
                    .ToList()
            });
        }
        public async Task<UserReportDTO> AddAsync(CreateReportDTO dto)
        {
            var entity = new UserReport
            {
                SenderUserId = dto.SenderUserId,
                ReportedUserId = dto.ReportedUserId,
                ReportReason = dto.ReportReason,
                ReportStatusId = 1,
                ModeratedByUserId = null, 
                IsDeleted = false
            };

            var created = await _reportRepository.AddAsync(entity);
            return new UserReportDTO(created);
        }
        public async Task MarkReportsClosedAsync(string userId, string moderId)
        {
            try
            {
                var reports = await _reportRepository.GetByReportedIdAsync(userId);
                var reportsToClose = reports
                    .Where(r => r.ReportStatusId == 1 && !r.IsDeleted) //на рассмотрении
                    .ToList();

                if (!reportsToClose.Any())
                    return;

                foreach (var rep in reportsToClose)
                {
                    rep.ReportStatusId = 2; //отменено
                    rep.ModeratedByUserId = moderId;
                    await _reportRepository.UpdateAsync(rep);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отметке жалоб {userId}");
                throw;
            }
        }
        public async Task<UserReportDTO> UpdateAsync(UserReportDTO dto)
        {
            var entity = new UserReport
            {
                Id = dto.Id,
                SenderUserId = dto.SenderUserId,
                ReportedUserId = dto.ReportedUserId,
                ReportReason = dto.ReportReason,
                ModeratedByUserId = dto.ModeratedByUserId,
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
