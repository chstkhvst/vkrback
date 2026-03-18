using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;

namespace ASPNETCore.Application.Services
{
    public class CatalogService
    {
        private readonly ICatalogRepository _catalogRepository;
        public CatalogService(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        #region AttendanceStatus Methods
        public Task<IEnumerable<AttendanceStatus>> GetAttendanceStatusesAsync() =>
            _catalogRepository.GetAttendanceStatusesAsync();
        public Task<AttendanceStatus?> GetAttendanceStatusByIdAsync(int id) =>
            _catalogRepository.GetAttendanceStatusByIdAsync(id);
        #endregion

        #region ReportStatus Methods
        public Task<IEnumerable<ReportStatus>> GetAllReportStatusesAsync() =>
            _catalogRepository.GetReportStatusesAsync();
        public Task<ReportStatus?> GetReportStatusByIdAsync(int id) =>
            _catalogRepository.GetReportStatusByIdAsync(id);

        #endregion

        #region City Methods
        public Task<IEnumerable<City>> GetAllCitiesAsync() =>
            _catalogRepository.GetCitiesAsync();
        public Task<City?> GetCityByIdAsync(int id) =>
            _catalogRepository.GetCityByIdAsync(id);
        #endregion

        #region EventCategory Methods
        public Task<IEnumerable<EventCategory>> GetAllEventCategoriesAsync() =>
            _catalogRepository.GetEventCategoriesAsync();
        public Task<EventCategory?> GetEventCategoryByIdAsync(int id) =>
            _catalogRepository.GetEventCategoryByIdAsync(id);
        #endregion

        #region EvensStatus Methods
        public Task<IEnumerable<EventStatus>> GetAllEventStatusesAsync () =>
            _catalogRepository.GetEventStatusesAsync();
        public Task<EventStatus?> GetEventStatusByIdAsync(int id) =>
            _catalogRepository.GetEventStatusByIdAsync(id);
        #endregion

        #region VolunteerRanks Methods
        public Task<IEnumerable<VolunteerRank>> GetAllVolunteerRanksAsync() =>
            _catalogRepository.GetVolunteerRanksAsync();
        public Task<VolunteerRank?> GetVolunteerRankByIdAsync(int id) =>
            _catalogRepository.GetVolunteerRankByIdAsync(id);
        #endregion
    }
}