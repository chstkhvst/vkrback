using ASPNETCore.Application.DTO;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ASPNETCore.Application.Services
{
    public class BanService
    {
        private readonly IBanRepository _banRepository;
        private readonly ReportService _reportService;
        private readonly ILogger<BanService> _logger;

        public BanService(IBanRepository banRepository, ReportService reportService, ILogger<BanService> logger)
        {
            _banRepository = banRepository;
            _logger = logger;
            _reportService = reportService;
        }

        public async Task<bool> IsUserBannedAsync(string userId)
        {
            return await _banRepository.IsUserBannedAsync(userId);
        }
        public async Task<BanDTO?> GetByIdAsync(int id)
        {
            var entity = await _banRepository.GetByIdAsync(id);
            return entity == null ? null : new BanDTO(entity);
        }

        public async Task<IEnumerable<BanDTO>> GetAllAsync(string? search)
        {
            var bans = await _banRepository.GetAllAsync(search);
            return bans.Select(b => new BanDTO(b));
        }

        public async Task<IEnumerable<BanDTO>> GetByUserIdAsync(string userId)
        {
            var bans = await _banRepository.GetByUserIdAsync(userId);
            return bans
                .Where(b => b != null)
                .Select(b => new BanDTO(b));
        }

        public async Task<IEnumerable<BanDTO>> GetByModerIdAsync(string moderId)
        {
            var bans = await _banRepository.GetByModerIdAsync(moderId);
            return bans
                .Where(b => b != null)
                .Select(b => new BanDTO(b));
        }

        public async Task<BanDTO> AddAsync(CreateBanDTO dto)
        {
            try
            {
                var entity = new Ban
                {
                    BannedUserId = dto.BannedUserId,
                    ModerId = dto.ModerId,
                    BanReason = dto.BanReason,
                    IsActive = dto.IsActive,
                    IsDeleted = dto.IsDeleted
                };

                var created = await _banRepository.AddAsync(entity);
                await _reportService.MarkReportsClosedAsync(dto.BannedUserId, dto.ModerId);
                return new BanDTO(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании бана для пользователя {dto.BannedUserId}");
                throw;
            }
        }

        public async Task<BanDTO> UpdateAsync(BanDTO dto)
        {
            var entity = new Ban
            {
                Id = dto.Id,
                BanReason = dto.BanReason,
                BannedUserId = dto.BannedUserId,
                ModerId = dto.ModerId,
                IsActive = dto.IsActive,
                IsDeleted = dto.IsDeleted
            };

            try
            {
                var updated = await _banRepository.UpdateAsync(entity);
                return new BanDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ban {dto.Id}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _banRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ban {id}");
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                await _banRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting ban {id}");
                throw;
            }
        }
    }
}
