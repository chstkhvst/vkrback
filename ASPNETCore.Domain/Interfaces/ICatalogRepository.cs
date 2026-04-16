using ASPNETCore.Domain.Entities;

namespace ASPNETCore.Domain.Interfaces
{
    public interface ICatalogRepository
    {
        Task<IEnumerable<AttendanceStatus>> GetAttendanceStatusesAsync();
        Task<AttendanceStatus?> GetAttendanceStatusByIdAsync(int id);
        Task<IEnumerable<EventCategory>> GetEventCategoriesAsync();
        Task<EventCategory?> GetEventCategoryByIdAsync(int id);
        Task<IEnumerable<EventStatus>> GetEventStatusesAsync();
        Task<EventStatus?> GetEventStatusByIdAsync(int id);
        Task<IEnumerable<ReportStatus>> GetReportStatusesAsync();
        Task<ReportStatus?> GetReportStatusByIdAsync(int id);
        Task<IEnumerable<VolunteerRank>> GetVolunteerRanksAsync();
        Task<VolunteerRank?> GetVolunteerRankByIdAsync(int id);
        Task<VolunteerRank?> GetRankForPointsAsync(int points);
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<City?> GetCityByIdAsync(int id);
    }
}
