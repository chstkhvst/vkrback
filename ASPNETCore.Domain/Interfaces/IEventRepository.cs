using ASPNETCore.Domain.Entities;

namespace ASPNETCore.Domain.Interfaces
{
    public interface IEventRepository
    {
        Task<VolunteerEvent?> GetByIdAsync(int id);
        Task<IEnumerable<VolunteerEvent>> GetAllAsync();
        Task<IEnumerable<VolunteerEvent>> GetFilteredAsync(
            int? catId, int? cityId, string? keyWords, DateTime? dateTime);
        Task<IEnumerable<VolunteerEvent>> GetEventsByUserIdAsync(
            string userId);
        Task<VolunteerEvent> UpdateAsync(VolunteerEvent volunteerEvent);
        Task<VolunteerEvent> AddAsync(VolunteerEvent volunteerEvent);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<PaginatedResponse<VolunteerEvent>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime);
        Task<PaginatedResponse<VolunteerEvent>> GetPagedForUserAsync(
            string userId,
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime
        );
    }
}
